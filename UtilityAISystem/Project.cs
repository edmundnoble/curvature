﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace Curvature
{
    [DataContract(Namespace = "")]
    [KnownType(typeof(InputParameterEnumeration))]
    [KnownType(typeof(InputParameterNumeric))]
    [KnownType(typeof(InputParameterValueEnumeration))]
    [KnownType(typeof(InputParameterValueNumeric))]
    public class Project : INameable
    {
        [DataMember]
        public string ReadableName;

        [DataMember]
        public KnowledgeBase KB;

        [DataMember]
        public List<Archetype> Archetypes;

        [DataMember]
        public List<Behavior> Behaviors;

        [DataMember]
        public List<BehaviorSet> BehaviorSets;

        [DataMember]
        public List<InputAxis> Inputs;

        [DataMember]
        public List<Scenario> Scenarios;


        internal class DeletionEventArgs : EventArgs
        {
            internal object DeletedObject;
        }

        internal delegate void DeletionEventHandler(object sender, DeletionEventArgs args);
        internal event DeletionEventHandler ItemDelete;

        internal delegate void DialogRebuildNeededHandler();
        internal event DialogRebuildNeededHandler DialogRebuildNeeded;

        internal delegate void ProjectDirtiedHandler(bool dirty);
        internal event ProjectDirtiedHandler ProjectDirtied;

        internal bool IsDirty { get; private set; }

        private Dictionary<string, InputAxis> InputLookupByName;

        public Project()
        {
            ReadableName = "New Project";
            KB = new KnowledgeBase();
            Archetypes = new List<Archetype>();
            Inputs = new List<InputAxis>();
            Behaviors = new List<Behavior>();
            BehaviorSets = new List<BehaviorSet>();
            Scenarios = new List<Scenario>();

            InputLookupByName = new Dictionary<string, InputAxis>();
        }


        public void RegisterInput(InputAxis axis)
        {
            Inputs.Add(axis);
            InputLookupByName.Add(axis.ReadableName, axis);
        }

        public string GenerateNewInputAxisName()
        {
            string newname = "New input";

            int counter = 2;
            while (InputLookupByName.ContainsKey(newname))
            {
                newname = $"New input {counter}";
                ++counter;
            }

            return newname;
        }

        public void Rename(string name)
        {
            ReadableName = name;
            DialogRebuildNeeded?.Invoke();
        }

        public string GetName()
        {
            return ReadableName;
        }


        public void MarkDirty()
        {
            IsDirty = true;
            ProjectDirtied?.Invoke(IsDirty);
        }


        public void SaveToFile(string filename)
        {
            var settings = new DataContractSerializerSettings
            {
                PreserveObjectReferences = true
            };

            var serializer = new DataContractSerializer(typeof(Project), settings);

            var xmlSettings = new XmlWriterSettings
            {
                Indent = true
            };

            using (var file = XmlWriter.Create(new FileStream(filename, FileMode.Create), xmlSettings))
            {
                serializer.WriteObject(file, this);
            }

            IsDirty = false;
            ProjectDirtied?.Invoke(IsDirty);
        }

        public static Project Deserialize(string filename)
        {
            var settings = new DataContractSerializerSettings
            {
                PreserveObjectReferences = true
            };

            var deserializer = new DataContractSerializer(typeof(Project), settings);

            Project ret = null;
            using (var file = new FileStream(filename, FileMode.Open))
            {
                ret = deserializer.ReadObject(file) as Project;
            }

            ret.IsDirty = false;
            ret.ProjectDirtied?.Invoke(false);

            foreach (Scenario s in ret.Scenarios)
                s.ResetAgents(ret.KB);

            return ret;
        }

        public void Delete(KnowledgeBase.Record record)
        {
            KB.Records.Remove(record);

            foreach (var input in Inputs)
            {
                if (input.KBRec == record)
                    input.KBRec = null;
            }

            MarkDirty();
            ItemDelete(this, new DeletionEventArgs { DeletedObject = record });
        }

        public void Delete(InputAxis input)
        {
            Inputs.Remove(input);

            foreach (var b in Behaviors)
            {
                foreach (var c in b.Considerations)
                {
                    if (c.Input == input)
                        c.Input = null;
                }
            }

            MarkDirty();
            ItemDelete(this, new DeletionEventArgs { DeletedObject = input });
        }

        public void Delete(Behavior behavior)
        {
            Behaviors.Remove(behavior);

            foreach (var set in BehaviorSets)
            {
                set.EnabledBehaviors.Remove(behavior);
            }

            MarkDirty();
            ItemDelete(this, new DeletionEventArgs { DeletedObject = behavior });
        }

        public void Delete(BehaviorSet set)
        {
            BehaviorSets.Remove(set);

            foreach (var archetype in Archetypes)
            {
                archetype.BehaviorSets.Remove(set);
            }

            MarkDirty();
            ItemDelete(this, new DeletionEventArgs { DeletedObject = set });
        }

        public void Delete(Archetype archetype)
        {
            Archetypes.Remove(archetype);

            foreach (var scenario in Scenarios)
            {
                foreach (var agent in scenario.Agents)
                {
                    if (agent.AgentArchetype == archetype)
                        agent.AgentArchetype = null;
                }
            }

            MarkDirty();
            ItemDelete(this, new DeletionEventArgs { DeletedObject = archetype });
        }

        public void Delete(Scenario scenario)
        {
            Scenarios.Remove(scenario);

            MarkDirty();
            ItemDelete(this, new DeletionEventArgs { DeletedObject = scenario });
        }

        public void Delete(Consideration consideration)
        {
            foreach (var behavior in Behaviors)
            {
                behavior.Considerations.Remove(consideration);
            }

            MarkDirty();
            ItemDelete(this, new DeletionEventArgs { DeletedObject = consideration });
        }
    }
}
