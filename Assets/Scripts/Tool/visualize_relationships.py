import os
import re
import networkx as nx
import matplotlib.pyplot as plt

# 1. ディレクトリ内の.csファイルを検索し、それらを読み込む
def find_cs_files(directory):
    cs_files = []
    for root, _, files in os.walk(directory):
        for file in files:
            if file.endswith(".cs"):
                cs_files.append(os.path.join(root, file))
    return cs_files

# 2. クラスの情報を収集する
def extract_classes_from_file(file_path):
    classes = []
    with open(file_path, "r", encoding='utf-8') as file:
        content = file.read()
        class_matches = re.findall(r'class\s+(\w+)\s*(?:\s*:\s*(?:[\w.]+(?:\[\])?(?:\s*,\s*[\w.]+(?:\[\])?)*)?)?\s*{', content)
        for match in class_matches:
            class_name = match[0]
            classes.append(class_name)
    return classes

# 3. クラスの関係を解析する
def analyze_class_relationships(cs_files):
    G = nx.Graph()
    for file in cs_files:
        classes = extract_classes_from_file(file)
        for i in range(len(classes)):
            for j in range(i + 1, len(classes)):
                G.add_edge(classes[i], classes[j])
    return G

# 4. 関連性を可視化する
def visualize_class_relationships(G):
    pos = nx.spring_layout(G)
    nx.draw(G, pos, with_labels=True, node_size=500, font_size=8)
    plt.show()

if __name__ == "__main__":
    directory = "../"  # クラスファイルがあるディレクトリのパスを指定してください
    cs_files = find_cs_files(directory)
    class_relationships = analyze_class_relationships(cs_files)
    visualize_class_relationships(class_relationships)
