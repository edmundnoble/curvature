﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Curvature
{
    [DataContract(Namespace = "")]
    public class Consideration : INameable
    {
        [DataMember]
        public string ReadableName;

        [DataMember]
        public InputAxis Input;

        [DataMember]
        public ResponseCurve Curve;

        [DataMember]
        public List<InputParameter> Parameters;

        internal Consideration()
        {
        }

        public Consideration(string name)
        {
            ReadableName = name;
            Parameters = new List<InputParameter>();
            Curve = new ResponseCurve(ResponseCurve.CurveType.Linear, 1.0, 1.0, 0.0, 0.0);
        }

        public Control CreateEditorUI(Project project)
        {
            return null;
        }

        internal double Score(IInputBroker broker, Scenario.Context context)
        {
            return Curve.ComputeValue(broker.GetInputValue(Input, context));
        }

        public void Rename(string newname)
        {
            ReadableName = newname;
        }

        public string GetName()
        {
            return ReadableName;
        }
    }
}