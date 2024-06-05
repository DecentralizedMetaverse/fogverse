import os

# 指定されたディレクトリのパス
directory = "./"

# 出力ファイル名
output_file = "combined_cs_files.txt"

# ディレクトリ内のすべての .tsx、.css、.js ファイルを取得
cs_files = []
for root, dirs, files in os.walk(directory):
    for file in files:
        if file.endswith(".go") or file.endswith(".css") or file.endswith(".js"):
            cs_files.append(os.path.join(root, file))

output = ""
for file in cs_files:
    print(file)
    with open(file, "r", encoding="utf-8") as f:
        output += f"// File: {file}\n"
        output += f.read()
        output += "\n\n"    

# 出力ファイルを開く
with open(output_file, "w", encoding="utf-8") as outfile:
    outfile.write(output)

print(f"Combined {len(cs_files)} files into {output_file}.")
