﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Curvature
{
    public partial class EditWidgetInputAxis : UserControl
    {
        private InputAxis EditedAxis;
        private Project EditedProject;

        internal EditWidgetInputAxis(Project project, InputAxis axis)
        {
            InitializeComponent();
            EditedAxis = axis;
            EditedProject = project;

            InputAxisNameLabel.Text = "Input Axis: " + axis.ReadableName;
        }

        private void InputTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (InputTypeComboBox.SelectedIndex == 0 || InputTypeComboBox.SelectedIndex == 1)
            {
                DataSourceComboBox.Items.Clear();

                foreach (var rec in EditedProject.KB.Records)
                {
                    DataSourceComboBox.Items.Add(rec.ReadableName);
                }
            }
            else if (InputTypeComboBox.SelectedIndex == 2)
            {
                DataSourceComboBox.Items.Clear();

                DataSourceComboBox.Items.Add("Distance to target");
            }
        }
    }
}
