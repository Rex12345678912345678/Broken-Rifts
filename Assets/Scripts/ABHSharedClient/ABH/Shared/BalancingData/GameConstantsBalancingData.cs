using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ABH.Shared.Models.Generic;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
    [ProtoContract]
    public class GameConstantsBalancingData : IBalancingData
    {
        [ProtoMember(1)]
        public string NameId { get; set; }

        [ProtoMember(2)]
        public string StringValue { get; set; }

        [ProtoMember(3)]
        public float FloatValue { get; set; }

        [ProtoMember(4)]
        public Requirement RequirementValue { get; set; }

        [ProtoMember(5)]
        public List<float> FloatlistValue { get; set; }

        [ProtoMember(6)]
        public bool BoolValue { get; set; }
    }
}