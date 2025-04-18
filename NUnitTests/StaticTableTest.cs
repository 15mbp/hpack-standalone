using NUnit.Framework;

namespace hpack
{
	public class StaticTableTest
	{
        [Test]
		public void testGetIndexByName()
		{
            int indexOfAuthority = StaticTable.GetIndex(System.Text.Encoding.UTF8.GetBytes(":authority"));
            Assert.That(indexOfAuthority, Is.EqualTo(1));
        }

        [Test]
		public void testGetIndexByUnknownName()
		{
            int indexOfInvalid = StaticTable.GetIndex(System.Text.Encoding.UTF8.GetBytes(":invalid"));
            Assert.That(indexOfInvalid, Is.EqualTo(-1));
        }
    }
}
