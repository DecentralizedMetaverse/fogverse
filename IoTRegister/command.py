import subprocess

def exe(command):
    output = subprocess.run(command, shell=True, capture_output=True, text=True).stdout
    return output