import os
import re
import shutil

# chatgpt wrote this script to rename all the files and occurrences of the string inside the files
# so I could rename the project more easily

def is_binary_file(filepath):
    """Check if a file is binary."""
    try:
        with open(filepath, 'rb') as file:
            chunk = file.read(1024)
            return b'\0' in chunk
    except Exception as e:
        print(f"Error reading file {filepath}: {e}")
        return True

def find_and_replace_in_file(filepath, find_str, replace_str):
    """Replace occurrences of find_str with replace_str in a file."""
    replacements = 0
    try:
        with open(filepath, 'r', encoding='utf-8') as file:
            content = file.read()
        
        replacements = content.count(find_str)
        updated_content = content.replace(find_str, replace_str)
        
        if replacements > 0:
            with open(filepath, 'w', encoding='utf-8') as file:
                file.write(updated_content)
    except Exception as e:
        print(f"Error processing file {filepath}: {e}")
        replacements = 0
    
    return replacements

def find_and_replace_in_names(directory, find_str, replace_str):
    """Replace occurrences of find_str with replace_str in file and folder names."""
    rename_operations = []
    for root, dirs, files in os.walk(directory, topdown=False):
        for name in files + dirs:
            if find_str in name:
                old_path = os.path.join(root, name)
                new_name = name.replace(find_str, replace_str)
                new_path = os.path.join(root, new_name)
                rename_operations.append((old_path, new_path))
    
    return rename_operations

def confirm_and_execute(rename_operations):
    """Check for collisions and execute rename operations if confirmed."""
    collisions = []
    for old_path, new_path in rename_operations:
        if os.path.exists(new_path):
            collisions.append((old_path, new_path))

    if collisions:
        print("WARNING: The following name collisions will occur:")
        for old_path, new_path in collisions:
            print(f"{old_path} -> {new_path}")

    print(f"{len(rename_operations)} rename operations detected.")
    if collisions:
        print("Resolve these collisions before proceeding.")
        return False

    proceed = input("Do you want to proceed with renaming? (y/n): ").strip().lower() == 'y'
    if proceed:
        for old_path, new_path in rename_operations:
            try:
                shutil.move(old_path, new_path)
            except Exception as e:
                print(f"Error renaming {old_path} to {new_path}: {e}")

    return proceed

def main():
    directory = input("Enter the directory: ").strip()
    find_str = input("Enter the string to find: ").strip()
    replace_str = input("Enter the string to replace with: ").strip()

    if not os.path.isdir(directory):
        print("The provided directory does not exist or is not a directory.")
        return

    # Track statistics
    total_file_replacements = 0

    # Process file contents
    for root, _, files in os.walk(directory):
        for file in files:
            filepath = os.path.join(root, file)
            if not is_binary_file(filepath):
                replacements = find_and_replace_in_file(filepath, find_str, replace_str)
                total_file_replacements += replacements

    print(f"Total occurrences replaced inside files: {total_file_replacements}")

    # Process file and folder names
    rename_operations = find_and_replace_in_names(directory, find_str, replace_str)
    proceed = confirm_and_execute(rename_operations)

    if proceed:
        print("Renaming completed.")
    else:
        print("Operation canceled.")

if __name__ == "__main__":
    main()
