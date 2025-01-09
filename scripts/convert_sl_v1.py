import os, tempfile, json, sys

data = sys.stdin.read()
data = json.loads(data)

result = {
    'mappings': { },
}

aliases = {}
for tup in data['keys']:
    k, v = tup['key'], tup['value']
    aliases.setdefault(v, set()).add(k)
for k, v in aliases.items():
    result['mappings'][k] = {
        'aliases': list(v)
    }

keymaps = {
    'attributes.category_name': '+tags',
    'attributes.budget_name': 'category',
    'attributes.bill_name': '+tags',
    'attributes.biil_name': '+tags',
    'add_tags': '+tags'
}
for tup in data['values']:
    key, value = tup['key'], json.loads(tup['value'])
    output_value = {}
    input_value = {}
    for k1, v1 in value.items():
        if isinstance(v1, dict):
            for k2, v2 in v1.items():
                input_value[f"{k1}.{k2}"] = v2
        else:
            input_value[k1] = v1

    for k, v in input_value.items():
        if v is None:
            continue
        mapped_key = keymaps[k]
        if mapped_key.startswith('+'):
            mapped_key = mapped_key[1:]
            if isinstance(v, list):
                if len(v) == 0:
                    continue
                existing  = output_value.get(mapped_key, []) 
                output_value[mapped_key] = list(set(existing + v))
            else:
                output_value.setdefault(mapped_key, []).append(v)
        else:
            output_value[mapped_key] = v
    if len(output_value) > 0:
        result['mappings'].setdefault(key, {})['value'] = json.dumps(output_value, indent=2)

        



print(json.dumps(result, indent=4))

