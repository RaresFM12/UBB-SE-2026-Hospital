import subprocess
import re
import os

services_dir = r"d:\facultate\UBB-cs\sem4\iss\UBB-SE-2026-Hospital\UBB-SE-2026-Hospital\Hospital.Services"
shared_services_dir = r"d:\facultate\UBB-cs\sem4\iss\UBB-SE-2026-Hospital\UBB-SE-2026-Hospital\Hospital.Shared\Services"

def run_build():
    print("Running dotnet build...")
    result = subprocess.run(
        ["C:\\Program Files\\dotnet\\dotnet.exe", "build", os.path.join(services_dir, "Hospital.Services.csproj")],
        capture_output=True,
        text=True
    )
    return result.stdout

def find_interface_file(interface_name):
    for root, _, files in os.walk(shared_services_dir):
        if f"{interface_name}.cs" in files:
            return os.path.join(root, f"{interface_name}.cs")
    return None

def extract_method_signature(interface_path, member_str):
    # member_str looks like 'SimulateIncomingRequestsAsync(int)'
    # or 'GetPendingRequestIdsAsync()'
    # or a property like 'CurrentUser'
    
    # We will search the interface file for this member.
    # It could be multi-line, so we read the whole file.
    with open(interface_path, "r", encoding="utf-8") as f:
        content = f.read()
    
    # If it's a method:
    if "(" in member_str:
        method_name = member_str.split("(")[0].strip()
        # Regex to find: return_type method_name(...)
        pattern = r"([^\s;>]+(?:\s*<[^>]+>)?\s+" + re.escape(method_name) + r"\s*\([^)]*\))\s*;"
        match = re.search(pattern, content)
        if match:
            return "public " + match.group(1).strip() + " { throw new System.NotImplementedException(); }"
        
        # Fallback regex without strict return type matching
        pattern2 = r"((?:[\w<>,\[\]]+\s+)+" + re.escape(method_name) + r"\s*\([^)]*\))\s*;"
        match2 = re.search(pattern2, content)
        if match2:
            return "public " + match2.group(1).strip() + " { throw new System.NotImplementedException(); }"
    else:
        # It's a property
        prop_name = member_str.strip()
        pattern = r"([^\s;>]+(?:\s*<[^>]+>)?\s+" + re.escape(prop_name) + r"\s*\{\s*get[^}]*\})"
        match = re.search(pattern, content)
        if match:
            return "public " + match.group(1).strip() + " => throw new System.NotImplementedException();"
            
    return None

def main():
    while True:
        output = run_build()
        errors = re.findall(r"(.*\.cs)\(\d+,\d+\): error CS0535: '([^']+)' does not implement interface member '([^.]+)\.([^']+)'", output)
        
        if not errors:
            print("No more CS0535 errors!")
            break
            
        print(f"Found {len(errors)} errors in this pass.")
        fixed_any = False
        
        for file_path, class_name, interface_name, member_str in set(errors):
            interface_path = find_interface_file(interface_name)
            if not interface_path:
                print(f"Could not find interface {interface_name}")
                continue
                
            sig = extract_method_signature(interface_path, member_str)
            if not sig:
                print(f"Could not extract signature for {member_str} in {interface_name}")
                continue
                
            print(f"Patching {class_name} with {member_str}")
            with open(file_path, "r", encoding="utf-8") as f:
                lines = f.readlines()
                
            # Find the last closing brace of the class
            for i in range(len(lines)-1, -1, -1):
                if "}" in lines[i]:
                    lines.insert(i, f"    {sig}\n")
                    break
                    
            with open(file_path, "w", encoding="utf-8") as f:
                f.writelines(lines)
                
            fixed_any = True
            
        if not fixed_any:
            print("Failed to fix any errors in this pass. Aborting.")
            break

if __name__ == "__main__":
    main()
