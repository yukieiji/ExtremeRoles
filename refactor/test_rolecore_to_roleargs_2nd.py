import unittest
import os
import tempfile
import shutil

# Assuming refactor_constructors.py is in the same directory
from refactor.rolecore_to_roleargs_2nd import refactor_content, refactor_csharp_files

class TestRefactorScript(unittest.TestCase):

    def test_refactor_content_single_occurrence(self):
        """Tests that a single instance of RoleCore.Build is replaced."""
        original = 'public Teleporter() : base(RoleCore.BuildCrewmate(...))'
        expected = 'public Teleporter() : base(RoleArgs.BuildCrewmate(...))'
        self.assertEqual(refactor_content(original), expected)

    def test_refactor_content_no_occurrence(self):
        """Tests that the content remains unchanged if the target string is not present."""
        original = 'public class MyClass { }'
        self.assertEqual(refactor_content(original), original)

    def test_refactor_content_multiple_occurrences(self):
        """Tests that all instances of RoleCore.Build are replaced."""
        original = '''
        var args1 = RoleCore.BuildImpostor(id);
        // Some comments
        var args2 = RoleCore.BuildCrewmate(id, color);
        '''
        expected = '''
        var args1 = RoleArgs.BuildImpostor(id);
        // Some comments
        var args2 = RoleArgs.BuildCrewmate(id, color);
        '''
        self.assertEqual(refactor_content(original), expected)

    def test_refactor_content_empty_string(self):
        """Tests that an empty string is handled correctly."""
        self.assertEqual(refactor_content(""), "")

class TestRefactorFileSystem(unittest.TestCase):

    def setUp(self):
        """Set up a temporary directory for file system tests."""
        self.test_dir = tempfile.mkdtemp()

    def tearDown(self):
        """Clean up the temporary directory after tests."""
        shutil.rmtree(self.test_dir)

    def _create_file(self, filename, content):
        """Helper function to create a file in the temporary directory."""
        filepath = os.path.join(self.test_dir, filename)
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(content)
        return filepath

    def test_modifies_correct_file(self):
        """Tests that a file with the target string is modified."""
        content = "base(RoleCore.BuildCrewmate())"
        filepath = self._create_file("Target.cs", content)

        refactor_csharp_files(self.test_dir)

        with open(filepath, 'r', encoding='utf-8') as f:
            new_content = f.read()

        self.assertEqual(new_content, "base(RoleArgs.BuildCrewmate())")

    def test_does_not_modify_unrelated_file(self):
        """Tests that a file without the target string is not modified."""
        content = "class Unrelated {}"
        filepath = self._create_file("Unrelated.cs", content)

        refactor_csharp_files(self.test_dir)

        with open(filepath, 'r', encoding='utf-8') as f:
            new_content = f.read()

        self.assertEqual(new_content, content)

    def test_skips_excluded_files(self):
        """Tests that RoleCore.cs and RoleArgs.cs are skipped even if they contain the target."""
        rolecore_content = "public static RoleCore BuildImpostor() => RoleCore.BuildImpostor();"
        roleargs_content = "public static RoleArgs BuildImpostor() => new RoleArgs(RoleCore.BuildImpostor());"

        rolecore_path = self._create_file("RoleCore.cs", rolecore_content)
        roleargs_path = self._create_file("RoleArgs.cs", roleargs_content)

        refactor_csharp_files(self.test_dir)

        with open(rolecore_path, 'r', encoding='utf-8') as f:
            self.assertEqual(f.read(), rolecore_content)

        with open(roleargs_path, 'r', encoding='utf-8') as f:
            self.assertEqual(f.read(), roleargs_content)

if __name__ == "__main__":
    unittest.main()