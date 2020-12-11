using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace XIVChat_Desktop.Controls {
    public class SelectableTextBlock : TextBlock {
        static SelectableTextBlock() {
            FocusableProperty.OverrideMetadata(typeof(SelectableTextBlock), new FrameworkPropertyMetadata(true));
            TextEditorWrapper.RegisterCommandHandlers(typeof(SelectableTextBlock), true, true, true);

            // remove the focus rectangle around the control
            FocusVisualStyleProperty.OverrideMetadata(typeof(SelectableTextBlock), new FrameworkPropertyMetadata(null));
        }

        protected SelectableTextBlock() {
            TextEditorWrapper.CreateFor(this);
        }
    }

    class TextEditorWrapper {
        private static readonly Type TextEditorType = Type.GetType("System.Windows.Documents.TextEditor, PresentationFramework")!;
        private static readonly PropertyInfo IsReadOnlyProp = TextEditorType.GetProperty("IsReadOnly", BindingFlags.Instance | BindingFlags.NonPublic)!;
        private static readonly PropertyInfo TextViewProp = TextEditorType.GetProperty("TextView", BindingFlags.Instance | BindingFlags.NonPublic)!;

        private static readonly MethodInfo RegisterMethod = TextEditorType.GetMethod(
            "RegisterCommandHandlers",
            BindingFlags.Static | BindingFlags.NonPublic,
            null,
            new[] {
                typeof(Type), typeof(bool), typeof(bool), typeof(bool),
            },
            null
        )!;

        private static readonly Type TextContainerType = Type.GetType("System.Windows.Documents.ITextContainer, PresentationFramework")!;
        private static readonly PropertyInfo TextContainerTextViewProp = TextContainerType.GetProperty("TextView")!;

        private static readonly PropertyInfo TextContainerProp = typeof(TextBlock).GetProperty("TextContainer", BindingFlags.Instance | BindingFlags.NonPublic)!;

        public static void RegisterCommandHandlers(Type controlType, bool acceptsRichContent, bool readOnly, bool registerEventListeners) {
            RegisterMethod.Invoke(null, new object[] {
                controlType, acceptsRichContent, readOnly, registerEventListeners,
            });
        }

        public static void CreateFor(TextBlock tb) {
            var textContainer = TextContainerProp.GetValue(tb);

            var editor = new TextEditorWrapper(textContainer!, tb, false);
            IsReadOnlyProp.SetValue(editor.editor, true);
            TextViewProp.SetValue(editor.editor, TextContainerTextViewProp.GetValue(textContainer));
        }

        private readonly object editor;

        private TextEditorWrapper(object textContainer, FrameworkElement uiScope, bool isUndoEnabled) {
            this.editor = Activator.CreateInstance(
                TextEditorType,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.CreateInstance,
                null,
                new[] {
                    textContainer, uiScope, isUndoEnabled,
                },
                null
            )!;
        }
    }
}
