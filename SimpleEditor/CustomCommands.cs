using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace SimpleEditor
{
    public static class FileCommands
    {
        public static readonly RoutedUICommand SaveAll = new RoutedUICommand(
            "Alles speichern",
            "SaveAll",
            typeof(FileCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.S, ModifierKeys.Alt | ModifierKeys.Control, "Strg + Alt + S")
            }
        );

        public static readonly RoutedUICommand CloseAll = new RoutedUICommand(
            "Alles schließen",
            "CloseAll",
            typeof(FileCommands)
        );

        public static readonly RoutedUICommand OpenFile = new RoutedUICommand(
            "Datei öffnen",
            "OpenFile",
            typeof(FileCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.O, ModifierKeys.Control, "Strg + O")
            }
        );

        public static readonly RoutedUICommand OpenDirectory = new RoutedUICommand(
            "Ordner öffnen",
            "OpenDirectory",
            typeof(FileCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.O, ModifierKeys.Control | ModifierKeys.Alt, "Strg + O")
            }
        );
    }
}