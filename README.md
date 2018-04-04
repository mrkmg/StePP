StePP
======

StePP is a overly simple task runner with concurrency and simple dependencies.


Install
-------

Coming Soon


Build
-----

Coming Soon


Usage
-----

Create a config file. The following is an example with all available options:

config.yaml
```yaml
name: Sample StePP Routine
version: 1

actions:
  some-script:
    executable: ./relative-script
    arguments:
      - Arg1
      - Arg2 With Space
    environment:
      KEY: SOMEKEY
      
  sleep-10:
    executable: sleep
    arguments:
      - 10
    canBeKilled: false
    ignoreFailure: false
      
  another-script: 
    executable: /absolute/path/to/script
  
steps:
  step-1:
    actions:
      - some-script
      - sleep-10
    logPath: ./logs/step-1
    canBeKilled: false
    
  step-2:
    actions:
      - another-script
    prerequisites:
      - step-1
```

Once you have a config file saved and setup, run the application:

StePP -c ./path/to/config.yaml

Config
------

Root options:

- `name`    - A string to name this config file.
- `version` - Config file version. For now always set to 1.
- `actions` - A named list of all actions. See Actions. 
- `steps`   - A named list of all steps. See Steps. 
- `logPath` - A path to write the output. If not set, or set to "-", then it outputs to STDOUT.

Action Options:

- `executable`  - The path to the executable.
- `arguments`   - An array of all arguments to pass to the executable.
- `environment` - A Key/Value set of Environment Variables
- `canBeKilled` - Whether or not this action can be killed. Default true.
- `ignoreFailure` - Whether or not to ignore failure of this action. Default false.

Step Options:

- `actions`       - A list of actions to run.
- `prerequisites` - A list of steps which must complete before this step runs.
- `logPath`       - A path to write the output of the step. If not set, or set to "-", then it outputs to Root logPath.
- `canBeKilled`   - true/false whether or not this action can be killed.
- `ignoreFailure` - Whether or not to ignore failure of this step. Default false.

LICENSE
-------

Copyright 2018 Kevin Gravier <kevin@mrkmg.com>

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
