using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WpfTextBox = System.Windows.Controls.TextBox;
using WpfComboBox = System.Windows.Controls.ComboBox;
using WpfDatePicker = System.Windows.Controls.DatePicker;
using WpfControl = System.Windows.Controls.Control;
using WpfBrushes = System.Windows.Media.Brushes;
using WpfColor = System.Windows.Media.Color;
using WpfSystemColors = System.Windows.SystemColors;
using WpfMessageBox = System.Windows.MessageBox;

namespace CustomerIssuesManager
{
    public static class ValidationHelper
    {
        /// <summary>
        /// Validates a required text field and provides visual feedback
        /// </summary>
        /// <param name="textBox">The TextBox to validate</param>
        /// <param name="fieldName">The name of the field for error messages</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool ValidateRequiredField(WpfTextBox textBox, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.BorderBrush = WpfBrushes.Red;
                textBox.BorderThickness = new Thickness(2);
                textBox.ToolTip = $"{fieldName} مطلوب";
                textBox.Background = new SolidColorBrush(WpfColor.FromArgb(20, 231, 76, 60)); // Light red background
                return false;
            }
            
            ClearValidationError(textBox);
            return true;
        }

        /// <summary>
        /// Validates a required ComboBox field and provides visual feedback
        /// </summary>
        /// <param name="comboBox">The ComboBox to validate</param>
        /// <param name="fieldName">The name of the field for error messages</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool ValidateRequiredComboBox(WpfComboBox comboBox, string fieldName)
        {
            if (comboBox.SelectedItem == null)
            {
                comboBox.BorderBrush = WpfBrushes.Red;
                comboBox.BorderThickness = new Thickness(2);
                comboBox.ToolTip = $"{fieldName} مطلوب";
                comboBox.Background = new SolidColorBrush(WpfColor.FromArgb(20, 231, 76, 60)); // Light red background
                return false;
            }
            
            ClearValidationError(comboBox);
            return true;
        }

        /// <summary>
        /// Validates a required DatePicker field and provides visual feedback
        /// </summary>
        /// <param name="datePicker">The DatePicker to validate</param>
        /// <param name="fieldName">The name of the field for error messages</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool ValidateRequiredDatePicker(WpfDatePicker datePicker, string fieldName)
        {
            if (datePicker.SelectedDate == null)
            {
                datePicker.BorderBrush = WpfBrushes.Red;
                datePicker.BorderThickness = new Thickness(2);
                datePicker.ToolTip = $"{fieldName} مطلوب";
                datePicker.Background = new SolidColorBrush(WpfColor.FromArgb(20, 231, 76, 60)); // Light red background
                return false;
            }
            
            ClearValidationError(datePicker);
            return true;
        }

        /// <summary>
        /// Validates subscriber number format
        /// </summary>
        /// <param name="textBox">The TextBox containing the subscriber number</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool ValidateSubscriberNumber(WpfTextBox textBox)
        {
            string subscriberNumber = textBox.Text?.Trim() ?? "";
            
            if (string.IsNullOrEmpty(subscriberNumber))
            {
                SetValidationError(textBox, "رقم المشترك مطلوب");
                return false;
            }

            // Basic subscriber number validation (numeric)
            if (!System.Text.RegularExpressions.Regex.IsMatch(subscriberNumber, @"^\d+$"))
            {
                SetValidationError(textBox, "رقم المشترك يجب أن يكون أرقام فقط");
                return false;
            }
            
            ClearValidationError(textBox);
            return true;
        }

        /// <summary>
        /// Validates debt amount format
        /// </summary>
        /// <param name="textBox">The TextBox containing the debt amount</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool ValidateDebtAmount(WpfTextBox textBox)
        {
            string debtAmount = textBox.Text?.Trim() ?? "";
            
            if (string.IsNullOrEmpty(debtAmount))
            {
                SetValidationError(textBox, "المديونية مطلوبة");
                return false;
            }

            // Validate decimal format
            if (!decimal.TryParse(debtAmount, out decimal amount) || amount < 0)
            {
                SetValidationError(textBox, "المديونية يجب أن تكون رقم موجب");
                return false;
            }
            
            ClearValidationError(textBox);
            return true;
        }

        /// <summary>
        /// Validates meter reading format
        /// </summary>
        /// <param name="textBox">The TextBox containing the meter reading</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool ValidateMeterReading(WpfTextBox textBox)
        {
            string meterReading = textBox.Text?.Trim() ?? "";
            
            if (string.IsNullOrEmpty(meterReading))
            {
                SetValidationError(textBox, "آخر قراءة عداد مطلوبة");
                return false;
            }

            // Validate numeric format
            if (!System.Text.RegularExpressions.Regex.IsMatch(meterReading, @"^\d+(\.\d+)?$"))
            {
                SetValidationError(textBox, "قراءة العداد يجب أن تكون رقم");
                return false;
            }
            
            ClearValidationError(textBox);
            return true;
        }

        /// <summary>
        /// Sets validation error styling on a control
        /// </summary>
        /// <param name="control">The control to style</param>
        /// <param name="errorMessage">The error message to display</param>
        private static void SetValidationError(WpfControl control, string errorMessage)
        {
            control.BorderBrush = WpfBrushes.Red;
            control.BorderThickness = new Thickness(2);
            control.ToolTip = errorMessage;
            control.Background = new SolidColorBrush(WpfColor.FromArgb(20, 231, 76, 60)); // Light red background
            
            // Add subtle glow effect
            control.Effect = new DropShadowEffect
            {
                Color = WpfColor.FromRgb(255, 0, 0), // Red color
                BlurRadius = 5,
                ShadowDepth = 0,
                Opacity = 0.3
            };
        }

        /// <summary>
        /// Clears validation error styling from a control
        /// </summary>
        /// <param name="control">The control to clear styling from</param>
        public static void ClearValidationError(WpfControl control)
        {
            control.BorderBrush = WpfSystemColors.ControlBrush;
            control.BorderThickness = new Thickness(1);
            control.ToolTip = null;
            control.Background = WpfSystemColors.WindowBrush;
            control.Effect = null;
        }

        /// <summary>
        /// Validates all required fields in a form
        /// </summary>
        /// <param name="controls">Dictionary of controls and their field names</param>
        /// <returns>True if all fields are valid, false otherwise</returns>
        public static bool ValidateAllRequiredFields(Dictionary<WpfControl, string> controls)
        {
            bool isValid = true;
            
            foreach (var control in controls)
            {
                bool fieldValid = false;
                
                switch (control.Key)
                {
                    case WpfTextBox textBox:
                        fieldValid = ValidateRequiredField(textBox, control.Value);
                        break;
                    case WpfComboBox comboBox:
                        fieldValid = ValidateRequiredComboBox(comboBox, control.Value);
                        break;
                    case WpfDatePicker datePicker:
                        fieldValid = ValidateRequiredDatePicker(datePicker, control.Value);
                        break;
                }
                
                if (!fieldValid)
                {
                    isValid = false;
                }
            }
            
            return isValid;
        }

        /// <summary>
        /// Shows a validation summary with all errors
        /// </summary>
        /// <param name="errors">List of error messages</param>
        public static void ShowValidationSummary(List<string> errors)
        {
            if (errors.Count > 0)
            {
                string errorMessage = "يرجى تصحيح الأخطاء التالية:\n\n" + string.Join("\n", errors);
                WpfMessageBox.Show(errorMessage, "أخطاء في التحقق", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
} 