using System;
using System.Collections.Generic;
using Hospital.Shared.Models;
using Hospital.Shared.Repositories;
using Hospital.Shared.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class PrescriptionServiceTests
    {
        [TestMethod]
        public void GetItemsFromPrescription_InvalidId_ThrowsArgumentException()
        {
            var itemsRepo = new FakeItemsRepository(new List<Item>());
            var evalRepo = new FakeEvaluationsRepository(new List<MedicalEvaluation>());
            var svc = new PrescriptionService(itemsRepo, evalRepo);

            try
            {
                svc.GetItemsFromPrescription("not-a-number", new Dictionary<int, float>());
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException)
            {
                // success
            }
        }

        [TestMethod]
        public void GetItemsFromPrescription_ValidEvaluation_ReturnsItems()
        {
            var preferred = new Item
            {
                Id = 10,
                Name = "MedA",
                NumberOfPills = 10,
                Price = 5f,
                Quantity = 2,
                ActiveSubstances = new Dictionary<string, float>()
            };

            var items = new List<Item> { preferred };
            var nameLookup = new Dictionary<string, List<Item>> { { "MedA", new List<Item> { preferred } } };
            var itemsRepo = new FakeItemsRepository(items, nameLookup);

            var eval = new MedicalEvaluation { EvaluationID = 1, MedicationsList = "MedA" };
            var evalRepo = new FakeEvaluationsRepository(new List<MedicalEvaluation> { eval });

            var svc = new PrescriptionService(itemsRepo, evalRepo);

            var result = svc.GetItemsFromPrescription("1", new Dictionary<int, float>());

            Assert.IsTrue(result.ContainsKey(10));
        }

        // --- fake repositories ---
        private sealed class FakeItemsRepository : IItemsRepository
        {
            private readonly List<Item> items;
            private readonly Dictionary<string, List<Item>> nameLookup;

            public FakeItemsRepository(List<Item> items, Dictionary<string, List<Item>>? nameLookup = null)
            {
                this.items = items;
                this.nameLookup = nameLookup ?? new Dictionary<string, List<Item>>();
            }

            public void AddItem(string name, string producer, string category, float price, int numberOfPills, string label = "", string description = "", string imagePath = "", float discount = 0f) => throw new NotImplementedException();
            public void AddItemWithQuantity(string name, string producer, string category, float price, int numberOfPills, int quantity, Dictionary<string, float> activeSubstances, Dictionary<DateOnly, int> batches, string label = "", string description = "", string imagePath = "", float discount = 0f) => throw new NotImplementedException();
            public void RemoveItemById(int itemIdToRemove) => throw new NotImplementedException();
            public Item GetItemById(int itemId) => this.items.Find(i => i.Id == itemId)!;
            public List<Item> GetAllItems() => new List<Item>(this.items);
            public List<Item> GetItemsByName(string name) => this.nameLookup.ContainsKey(name) ? new List<Item>(this.nameLookup[name]) : new List<Item>();
            public void UpdateItemById(Item newItem) => throw new NotImplementedException();
            public bool ItemExists(int itemId) => this.items.Exists(i => i.Id == itemId);
            public List<Tuple<int, string, int>> GetTop30Items() => new List<Tuple<int, string, int>>();
        }

        private sealed class FakeEvaluationsRepository : IEvaluationsRepository
        {
            private readonly List<MedicalEvaluation> evaluations;

            public FakeEvaluationsRepository(List<MedicalEvaluation> evaluations)
            {
                this.evaluations = evaluations;
            }

            public IReadOnlyList<MedicalEvaluation> GetAllEvaluations() => new List<MedicalEvaluation>(this.evaluations);

            public void AddEvaluation(int doctorId, int patientId, string diagnosis, string notes, string medications, bool assumedRisk)
            {
                // no-op for tests
            }

            public void UpdateEvaluation(int evaluationId, string diagnosis, string notes, string medications)
            {
                // no-op for tests
            }

            public void DeleteEvaluation(int evaluationId)
            {
                // no-op for tests
            }
        }

        [TestMethod]
        public void GetItemsFromPrescription_NoMatchingItems_ThrowsArgumentException()
        {
            var itemsRepo = new FakeItemsRepository(new List<Item>());
            var evalRepo = new FakeEvaluationsRepository(new List<MedicalEvaluation>());
            var svc = new PrescriptionService(itemsRepo, evalRepo);

            Assert.ThrowsException<ArgumentException>(() => svc.GetItemsFromPrescription("1", new Dictionary<int, float>()));
        }
    }
}
