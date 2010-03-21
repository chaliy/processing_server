using System.Runtime.Serialization;

namespace Task1Contract
{	
	[DataContract(Namespace = "urn:test:task1-v1.0")]
	public class Task1Msg2
	{
		[DataMember]
		public string Data { get; set; }
	}
}
