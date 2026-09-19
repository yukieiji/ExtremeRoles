import pytest
import os
import sys
from unittest.mock import patch
import fr_checker


def test_check_french_translations_default():
    result = fr_checker.check_french_translations(fr_checker.RESX_DIR)
    assert result is True


def test_check_french_translations_target_valid():
    result = fr_checker.check_french_translations(fr_checker.RESX_DIR, target_file="Text.fr-FR.resx")
    assert result is True


def test_check_french_translations_target_nonexistent():
    result = fr_checker.check_french_translations(fr_checker.RESX_DIR, target_file="NonExistent.fr-FR.resx")
    assert result is False


def test_main_default():
    with patch.object(sys, "argv", ["fr_checker.py"]):
        with patch("sys.exit") as mock_exit:
            fr_checker.main()
            mock_exit.assert_not_called()


def test_main_target_valid():
    with patch.object(sys, "argv", ["fr_checker.py", "--target", "Text.fr-FR.resx"]):
        with patch("sys.exit") as mock_exit:
            fr_checker.main()
            mock_exit.assert_not_called()


def test_main_target_nonexistent():
    with patch.object(sys, "argv", ["fr_checker.py", "--target", "NonExistent.fr-FR.resx"]):
        with patch("sys.exit") as mock_exit:
            fr_checker.main()
            mock_exit.assert_called_once_with(1)
