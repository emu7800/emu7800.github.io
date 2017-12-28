using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Phone.Controls;

using EMU7800.WP.Model;

namespace EMU7800.WP
{
    public class StateUtils
    {
        // A List of Actions that will be executed on the first render.
        // This is used to scroll the ScrollViewer to the correct offset.
        static List<Action> _workItems;

        /// <summary>
        /// Saves the contents and selection location of a TextBox to the state dictionary.
        /// </summary>
        /// <param name="pageState">The calling page's state dictionary.</param>
        /// <param name="textBox">The TextBox to be preserved.</param>
        public static void PreserveState(IDictionary<string, object> pageState, TextBox textBox)
        {
            pageState[textBox.Name + "_Text"] = textBox.Text;
            pageState[textBox.Name + "_SelectionStart"] = textBox.SelectionStart;
            pageState[textBox.Name + "_SelectionLength"] = textBox.SelectionLength;
        }

        /// <summary>
        /// Restores the contents and selection location of a TextBox from the page's state dictionary.
        /// </summary>
        /// <param name="pageState">The calling page's state dictionary.</param>
        /// <param name="textBox">The TextBox to be restored.</param>
        /// <param name="defaultValue">A default value that is used if the saved value cannot be retrieved.</param>
        public static void RestoreState(IDictionary<string, object> pageState, TextBox textBox, string defaultValue)
        {
            textBox.Text = TryGetValue(pageState, textBox.Name + "_Text", defaultValue);
            textBox.SelectionStart = TryGetValue(pageState, textBox.Name + "_SelectionStart", textBox.Text.Length);
            textBox.SelectionLength = TryGetValue(pageState, textBox.Name + "_SelectionLength", 0);
        }

        /// <summary>
        /// Saves the checked state of a CheckBox to the state dictionary.
        /// </summary>
        /// <param name="pageState">The calling page's state dictionary.</param>
        /// <param name="checkBox">The CheckBox to be preserved.</param>
        public static void PreserveState(IDictionary<string, object> pageState, CheckBox checkBox)
        {
            pageState[checkBox.Name + "_IsChecked"] = checkBox.IsChecked;
        }

        /// <summary>
        /// Restores the checked state of a CheckBox from the page's state dictionary.
        /// </summary>
        /// <param name="pageState">The calling page's state dictionary.</param>
        /// <param name="checkBox">The CheckBox to be restored.</param>
        /// <param name="defaultValue">A default value that is used if the saved value cannot be retrieved.</param>
        public static void RestoreState(IDictionary<string, object> pageState, CheckBox checkBox, bool defaultValue)
        {
            checkBox.IsChecked = TryGetValue(pageState, checkBox.Name + "_IsChecked", defaultValue);
        }

        /// <summary>
        /// Saves the value of a Slider to the state dictionary.
        /// </summary>
        /// <param name="pageState">The calling page's state dictionary.</param>
        /// <param name="slider">The Slider to be preserved.</param>
        public static void PreserveState(IDictionary<string, object> pageState, Slider slider)
        {
            pageState[slider.Name + "_Value"] = slider.Value;
        }

        /// <summary>
        /// Restores the value of a Slider from the page's state dictionary.
        /// </summary>
        /// <param name="pageState">The calling page's state dictionary.</param>
        /// <param name="slider">The Slider to be restored.</param>
        /// <param name="defaultValue">A default value that is used if the saved value cannot be retrieved.</param>
        public static void RestoreState(IDictionary<string, object> pageState, Slider slider, double defaultValue)
        {
            slider.Value = TryGetValue(pageState, slider.Name + "_Value", defaultValue);
        }

        /// <summary>
        /// Saves the checked state of a RadioButton to the state dictionary.
        /// </summary>
        /// <param name="pageState">The calling page's state dictionary.</param>
        /// <param name="radioButton">The RadioButton to be preserved.</param>
        public static void PreserveState(IDictionary<string, object> pageState, RadioButton radioButton)
        {
            pageState[radioButton.Name + "_IsChecked"] = radioButton.IsChecked;
        }

        /// <summary>
        /// Restores the checked state of a RadioButton from the page's state dictionary.
        /// </summary>
        /// <param name="pageState">The calling page's state dictionary.</param>
        /// <param name="radioButton">The RadioButton to be restored.</param>
        /// <param name="defaultValue">A default value that is used if the saved value cannot be retrieved.</param>
        public static void RestoreState(IDictionary<string, object> pageState, RadioButton radioButton, bool defaultValue)
        {
            radioButton.IsChecked = TryGetValue(pageState, radioButton.Name + "_IsChecked", defaultValue);
        }

        /// <summary>
        /// Saves the scroll offset of a ScrollViewer to the state dictionary.
        /// </summary>
        /// <param name="pageState">The calling page's state dictionary.</param>
        /// <param name="scrollViewer">The ScrollViewer to be preserved.</param>
        public static void PreserveState(IDictionary<string, object> pageState, ScrollViewer scrollViewer)
        {
            pageState[scrollViewer.Name + "_HorizontalOffset"] = scrollViewer.VerticalOffset;
            pageState[scrollViewer.Name + "_VerticalOffset"] = scrollViewer.VerticalOffset;
        }

        /// <summary>
        /// Retrieves the saved scroll offset from the page's state dictionary and creates a delegate to
        /// restore the scroll position on the page's first render.
        /// </summary>
        /// <param name="pageState"></param>
        /// <param name="scrollViewer"></param>
        public static void RestoreState(IDictionary<string, object> pageState, ScrollViewer scrollViewer)
        {
            var offset = TryGetValue<double>(pageState, scrollViewer.Name + "_HorizontalOffset", 0);
            if (offset > 0)
            {
                var horizOffset = offset;
                ScheduleOnNextRender(() => scrollViewer.ScrollToHorizontalOffset(horizOffset));
            }

            offset = TryGetValue<double>(pageState, scrollViewer.Name + "_VerticalOffset", 0);
            if (offset > 0)
            {
                var vertOffset = offset;
                ScheduleOnNextRender(() => scrollViewer.ScrollToVerticalOffset(vertOffset));
            }
        }

        /// <summary>
        /// Retrieves the saved current pivot item from the page's state dictionary.
        /// </summary>
        /// <param name="pageState"></param>
        /// <param name="pivot"></param>
        public static void PreserveState(IDictionary<string, object> pageState, Pivot pivot)
        {
            pageState[pivot.Name + "_SelectedIndex"] = pivot.SelectedIndex;
        }

        /// <summary>
        /// Saves the current pivot item to the state dictionary.
        /// </summary>
        /// <param name="pageState"></param>
        /// <param name="pivot"></param>
        /// <param name="defaultValue"></param>
        public static void RestoreState(IDictionary<string, object> pageState, Pivot pivot, int defaultValue)
        {
            pivot.SelectedIndex = TryGetValue(pageState, pivot.Name + "_SelectedIndex", defaultValue);
        }

        /// <summary>
        /// Saves the name of the control that has focus to the state dictionary.
        /// </summary>
        /// <param name="pageState">The calling page's state dictionary.</param>
        /// <param name="parent">The parent element for which focus is being saved.</param>
        public static void PreserveFocusState(IDictionary<string, object> pageState, FrameworkElement parent)
        {
            // Determine which control currently has focus.
            var focusedControl = FocusManager.GetFocusedElement() as Control;

            // If no control has focus, store null in the State dictionary.
            if (focusedControl == null)
            {
                pageState["FocusedControlName"] = null;
            }
            else
            {
                var foundCountrol = parent.FindName(focusedControl.Name) as Control;

                // If the control isn't found within the parent, store null in the State dictionary.
                if (foundCountrol == null)
                {
                    pageState["FocusedElementName"] = null;
                }
                else
                {
                    // otherwise store the name of the control with focus.
                    pageState["FocusedElementName"] = focusedControl.Name;
                }
            }
        }

        /// <summary>
        /// Retrieves the name of the control that should have focus and creates a delegate to
        /// restore the scroll position on the page's first render.
        /// </summary>
        /// <param name="pageState">The calling page's state dictionary.</param>
        /// <param name="parent">The parent element for which focus is being restored.</param>
        public static void RestoreFocusState(IDictionary<string, object> pageState, FrameworkElement parent)
        {
            // Get the name of the control that should have focus.
            var focusedName = TryGetValue<string>(pageState, "FocusedElementName", null);

            // Check to see if the name is null or empty
            if (String.IsNullOrEmpty(focusedName))
                return;

            // Find the control name in the parent.
            var focusedControl = parent.FindName(focusedName) as Control;
            if (focusedControl != null)
            {
                // If the control is found, schedule a call to its Focus method for the next render.
                ScheduleOnNextRender(() => focusedControl.Focus());
            }
        }

        /// <summary>
        /// Returns the filename for the specified <paramref name="gameProgramId"/> for use with isolated storage.
        /// </summary>
        /// <param name="gameProgramId"></param>
        public static string ToSerializationFileName(GameProgramId gameProgramId)
        {
            return String.Format("emu7800.machinestate.{0}.emu", gameProgramId);
        }

        #region Helpers

        static T TryGetValue<T>(IDictionary<string, object> pageState, string name, T defaultValue)
        {
            if (pageState.ContainsKey(name))
            {
                if (pageState[name] != null)
                {
                    return (T)pageState[name];
                }
            }
            return defaultValue;
        }

        // Adds the supplied action to a list of actions that will be performed on the next render. This is
        // used to schedule actions that cannot be completed before the page is rendered, such as setting 
        // the offset of a ScrollViewer.
        static void ScheduleOnNextRender(Action action)
        {
            // If the list of work items is null, create a new one and register DoWorkOnRender as a 
            // handler for the CompositionTarget.Rendering event.
            if (_workItems == null)
            {
                _workItems = new List<Action>();
                CompositionTarget.Rendering += DoWorkOnRender;
            }

            // Add the supplied action to the list.
            _workItems.Add(action);
        }

        // The event handler for the CompositionTarget.Rendering event. This handler invokes the actions
        // added with ScheduleOnNextRender. It deregisters itself from the Rendering event so that it is
        // only called once.
        static void DoWorkOnRender(object sender, EventArgs args)
        {
            // Remove ourselves from the event and clear the list
            CompositionTarget.Rendering -= DoWorkOnRender;
            List<Action> work = _workItems;
            _workItems = null;

            foreach (Action action in work)
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {

                    if (Debugger.IsAttached)
                        Debugger.Break();

                    Debug.WriteLine("Exception while doing work for " + action.Method.Name + ". " + ex.Message);
                }
            }
        }

        #endregion
    }
}
