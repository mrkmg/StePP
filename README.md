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

Create a yaml file for each routine. The following is an example with all available options:

```yaml
name: Sample Test
version: 1

actions:
  action1:
    executable: ./echo-sleep
    arguments:
      - |
        Action
        Action
      - 1

steps:
  one:
    actions:
      - action1

```
