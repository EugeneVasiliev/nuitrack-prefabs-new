const { task, series } = require('gulp');
const child_process = require('child_process');
const path = require('path');

const isWindows = process.platform === 'win32';

let _unityPath = null;

const findUnity = function(){   
  const unityVersion = process.env.UNITY_VERSION;
  let unityHubPath = process.env.UNITY_HUB_PATH;
  if(!unityHubPath){
    unityHubPath = isWindows ? 'C:/Program Files/Unity Hub/Unity Hub.exe' : '/Application/Unity Hub.app/Contents/MacOS/Unity Hub'
  }
  const data =  child_process.spawnSync(
    unityHubPath,
    [ '--', '--headless', 'editors', '-i' ],
    {encoding:'utf8'});
  const unityVersions = data.stdout.replace(/\r/g,'').split('\n');
  console.log('installed unity versions',unityVersions);
  const candidate = unityVersions.find(version => version.startsWith(unityVersion));
  if(!candidate){
    return Promise.reject(`Could not find unity ${unityVersion}`);
  }
  _unityPath = candidate.split('installed at ')[1];
  console.log('unity path',_unityPath);
  return Promise.resolve(_unityPath);
}

const buildUnity = function(){
    //https://docs.unity3d.com/Manual/CommandLineArguments.html
    const buildTargets = {
      'StandaloneWindows64': 'Win64',
      'StandaloneWindows':'Win',      
    }    
    const projectPath = path.resolve(process.cwd());    
    const escapedProjectPath = isWindows ? `"${projectPath}"` : projectPath.replace(/\s/g,'\\ ');    
    //todo add cache server localhost:8126,
    const unityEditorProcess=  child_process.spawn(
      _unityPath,
      [
        '-batchmode',
        '-quit',
        '-silent-crashes',
        '-logfile', '-', //log to std out
        '-projectPath', './',
        '-CacheServerIPAddress', process.env.UNITY_CACHE_SERVER,
        '-buildTarget', buildTargets[process.env.BUILD_TARGET],
        '-executeMethod', 'MyBuildScript.MyBuildProcess',
      ],
      {
        cwd:projectPath,
        windowsHide:true,
        label: 'Unity',
        env: process.env,
      });
    unityEditorProcess.stdout.pipe(process.stdout);
    unityEditorProcess.stderr.pipe(process.stderr);
    return unityEditorProcess;
}

task('buildWindows',
series(
  findUnity,
  buildUnity
  ));

task('default',findUnity);