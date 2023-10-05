import os
import codecs

def convert_encoding(filename, target_encoding):
    with codecs.open(filename, 'r', 'shift_jis') as file:
        content = file.read()
    with codecs.open(filename, 'w', target_encoding) as file:
        file.write(content)

def main():
    for root, dirs, files in os.walk('./../'):
        for filename in files:
            if filename.endswith('.cs'):
                print(filename)
                try: 
                    convert_encoding(os.path.join(root, filename), 'utf-8')
                except:
                    print("無理")

if __name__ == '__main__':
    main()