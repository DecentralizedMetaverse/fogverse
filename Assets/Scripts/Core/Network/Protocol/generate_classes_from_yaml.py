import os
import yaml

def generate_classes_from_yaml():
    data = read(f"{parent_path}/{yaml_file_name}")
    generate_classes(data)

def read(path):
    if not os.path.exists(path):
        return None

    with open(path, 'r') as file:
        input_data = file.read()
        result = read_text(input_data)

    return result

def read_text(input_data):
    result = yaml.safe_load(input_data)
    return result

def generate_classes(data):
    types = []
    messages = data.get("messages", [])
    for message in messages:
        class_name = message.get("name", "")
        types.append(class_name)
        fields = message.get("fields", [])
        generate_class(class_name, fields)
        
    generate_enum_file(types)
        
def generate_enum_file(types):
    txt = f"public enum {enum_file_name}\n"
    txt += "{\n"
    for type in types:
        txt += f"    {type},\n"
    txt += "}\n"
    with open(f"{parent_path}/{enum_file_name}.cs", 'w') as file:
        file.write(txt)

def generate_class(class_name, fields):
    class_name = f"P_{class_name}"
    generated_code = (
        "using System;\n"
        "using System.Collections.Generic;\n"
        "using MemoryPack;\n"
        "using UnityEngine;\n"
        "\n"
        f"[MemoryPackable]\n"
        f"public partial class {class_name}\n"
    )

    generated_code += "{\n"

    for field in fields:
        field_name = field.get("name", "")
        field_type = field.get("type", "")
        generated_code += f"    public {get_csharp_type(field_type)} {field_name} {{ get; set; }}\n"

    generated_code += "}\n"

    # C#のコードをファイルに保存
    with open(f"{parent_path}/{class_name}.cs", 'w') as file:
        file.write(generated_code)

def get_csharp_type(field_type):
    type_mapping = {
        "string": "string",
        "List<string>": "List<string>",
        "(int, int, int)": "(int, int, int)",
        "int": "int",
        "float": "float",
        "boolean": "bool",
        "Vector3": "Vector3",
    }

    return type_mapping.get(field_type, "object")

# 以下は適切な値を設定して呼び出す部分
parent_path = "./"
yaml_file_name = "protocol.yaml"
enum_file_name = "MessageType"
generate_classes_from_yaml()
