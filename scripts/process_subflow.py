import os, tempfile, json, sys

data = sys.stdin.read()
data = json.loads(data)

definition_node = data[0]
definition_node["flows"] = data[1:-1]
print(json.dumps(definition_node, indent=4))

