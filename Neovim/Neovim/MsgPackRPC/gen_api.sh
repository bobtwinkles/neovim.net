#!/bin/bash
nvim --api-info | python2 -c 'import msgpack, sys, yaml; print yaml.dump(msgpack.unpackb(sys.stdin.read()))' > api.yaml
