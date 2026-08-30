using ProtoBuf;

namespace ABH.Shared.BalancingData
{
    [ProtoContract]
    public class SplashScreenBalancingData
    {
        [ProtoMember(1)]
        public string NameId { get; set; }

        [ProtoMember(2)]
        public uint StartTimestamp { get; set; }

        [ProtoMember(3)]
        public uint EndTimestamp { get; set; }
    }
}