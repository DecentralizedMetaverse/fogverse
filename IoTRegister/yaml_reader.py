import yaml

def write(filename, obj):
    with open(filename, 'w', encoding="utf-8") as f:
        yaml.dump(obj, f, default_flow_style=False)
        
def read(filename):
    with open(filename, encoding="utf-8") as yml:
        obj = yaml.safe_load(yml)
        return obj
        