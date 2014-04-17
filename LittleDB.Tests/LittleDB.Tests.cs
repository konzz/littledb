using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Littledb.Tests
{
    [TestFixture]
    public class LittleDbTests
    {
        [Test]
        public void TheDBShouldHaveANameAndCreateADirectory()
        {
            new LittleDB("myDatabase");
            Assert.IsTrue(Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "myDatabase")));
        }

        [Test]
        public void IShouldBeAbleToInstantiateTwoDatabasesWithSameName()
        {
            new LittleDB("myDatabase");
            new LittleDB("myDatabase");
        }

        [Test]
        public void ShouldCreateAfileWithNamedAsTheTypeOfTheObjectSaved()
        {
            var littledb = new LittleDB("myDatabase");
            var me = new Programmer("alberto");
            littledb.Save(me);
            Assert.IsTrue(File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "myDatabase", "Programmer")));
        }

        [Test]
        public void WhenReadingShouldReturnACollectionOfObjects()
        {
            var littledb = new LittleDB("myDatabase");
            var me = new Programmer("alberto");
            littledb.Save(me);

            var programmers = littledb.GetAll<Programmer>();

            Assert.That(programmers.Count, Is.EqualTo(1));
            Assert.That(programmers[0].Name, Is.EqualTo(me.Name));
        }

        [Test]
        public void ItShouldReturnAnEmptyListIfNoObjectsSaved()
        {
            var littledb = new LittleDB("myDatabase");
            var programmers = littledb.GetAll<Programmer>();

            Assert.That(programmers.Count, Is.EqualTo(0));
        }

        [Test]
        public void ICanSaveMultiPleElements()
        {
            var littledb = new LittleDB("myDatabase");
            var me = new Programmer("alberto");
            var he = new Programmer("marcelo");
            littledb.Save(me);
            littledb.Save(he);

            List<Programmer> programmers = littledb.GetAll<Programmer>();

            Assert.That(programmers.Count, Is.EqualTo(2));
            Assert.That(programmers[0].Name, Is.EqualTo(me.Name));
            Assert.That(programmers[1].Name, Is.EqualTo(he.Name));
        }

        [Test]
        public void CanSaveAList()
        {
            var littledb = new LittleDB("myDatabase");
            var me = new Programmer("alberto");
            var he = new Programmer("marcelo");

            var programers = new List<Programmer>(){me, he};
            littledb.SaveList(programers);

            var programmers = littledb.GetAll<Programmer>();

            Assert.That(programmers.Count, Is.EqualTo(2));
            Assert.That(programmers[0].Name, Is.EqualTo(me.Name));
            Assert.That(programmers[1].Name, Is.EqualTo(he.Name));
        }

        [Test]
        public void ItShouldntCreateNewRegistryWhenSavingObjectsWithSameIdProperty()
        {
            var littledb = new LittleDB("myDatabase");
            var objectWithId = new ObjectWithId(23, "myObject");
            littledb.Save(objectWithId);
            littledb.Save(objectWithId);

            littledb.GetAll<ObjectWithId>().Count.ShouldBeEquivalentTo(1);
        }

        [Test]
        public void ItShouldGiveIdToObjectsWithThePropertyAndIdIsZero()
        {
            var littledb = new LittleDB("myDatabase");
            var objectWithId = new ObjectWithId(0, "myObject");
            littledb.Save(objectWithId);
            littledb.GetAll<ObjectWithId>().Last().Id.ShouldBeEquivalentTo(1);

            var anotherObjectWithId = new ObjectWithId(0, "myObject");
            littledb.Save(anotherObjectWithId);
            littledb.GetAll<ObjectWithId>().Last().Id.ShouldBeEquivalentTo(2);
        }

        [TearDown]
        public void RemoveDBFolder()
        {
            Directory.Delete("myDatabase", true);
        }

        class Programmer
        {
            public string Name { get; set; }

            public Programmer(string name)
            {
                Name = name;
            }
        }

        class ObjectWithId
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public ObjectWithId(int id, string name)
            {
                Id = id;
                Name = name;
            }
        }
    }
}
