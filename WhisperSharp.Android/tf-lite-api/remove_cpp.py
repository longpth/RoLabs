import os

# Set the directory path to the root of your repository
root_dir = '.'

# File extensions to remove
extensions = ['.c', '.cc', '.cpp']

# Traverse the directory tree
for root, dirs, files in os.walk(root_dir):
    for file in files:
        # Check if the file has one of the target extensions
        if any(file.endswith(ext) for ext in extensions):
            file_path = os.path.join(root, file)
            try:
                os.remove(file_path)
                print(f"Removed: {file_path}")
            except Exception as e:
                print(f"Error removing {file_path}: {e}")

print("Finished removing files.")
