using System;
using System.Collections.Generic;
using ABH.Shared.Models.Generic;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
    [ProtoContract]
    public class SetFusionBalancingData : IBalancingData
    {
        [ProtoMember(1)]
        public string NameId { get; set; }

        [ProtoMember(2)]
        public float ChanceWith3Different { get; set; }

        [ProtoMember(3)]
        public float ChanceWith2SameOn2 { get; set; }

        [ProtoMember(4)]
        public float ChanceWith2SameOn1 { get; set; }

        [ProtoMember(5)]
        public float ChanceWith3Same { get; set; }

        [ProtoMember(6)]
        public float BannerChanceWith3Same { get; set; }

        [ProtoMember(7)]
        public float BannerChanceWith2SameOn2 { get; set; }

        [ProtoMember(8)]
        public List<Requirement> BuyRequirements { get; set; }

        [ProtoMember(9)]
        public List<Requirement> RerollcostBase { get; set; }

        [ProtoMember(10)]
        public float RerollcostIncrease { get; set; }

        [ProtoMember(11)]
        public float RerollcostMax { get; set; }

        [ProtoMember(12)]
        public float AncientChance { get; set; }

        [ProtoMember(13)]
        public float AncientChanceRerollIncrease { get; set; }

        [ProtoMember(14)]
        public float AncientChanceRerollMax { get; set; }

        [ProtoMember(15)]
        public int AncientItemEnchLevel { get; set; }

        [ProtoMember(16)]
        public float IncreaseAncientChancePerAncientItem { get; set; }

        [ProtoMember(17)]
        public List<Requirement> BannerFusionBuyRequirements { get; set; }

        [ProtoMember(18)]
        public List<Requirement> RerollBannerCostBase { get; set; }

        [ProtoMember(19)]
        public float RerollBannerCostIncrease { get; set; }

        [ProtoMember(20)]
        public float RerollBannerCostMax { get; set; }
    }
}