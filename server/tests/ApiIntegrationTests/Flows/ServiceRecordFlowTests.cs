// using ApiIntegrationTests.Contracts;
// using ApiIntegrationTests.Contracts.V1;
// using ApiIntegrationTests.Fixtures;
// using Application.Core;
// using Application.ServiceRecords;
// using Domain.Enums.Services;
// using System.Net;
// using System.Net.Http.Json;
//
// namespace ApiIntegrationTests.Flows;
//
// [Collection("ServiceRecordFlowTests")]
// public class ServiceRecordFlowTests : BaseIntegrationTest
// {
//     public ServiceRecordFlowTests(CustomWebApplicationFactory factory) : base(factory)
//     {
//     }
//
//     [Fact]
//     public async Task ServiceRecordLifecycleFlow_WithoutServiceItems_FullCRUD()
//     {
//         // Setup
//         await CreateAndAuthenticateUser();
//         var vehicle = await CreateVehicleAsync();
//         var serviceType1 = await CreateServiceTypeAsync("Oil Change");
//         var serviceType2 = await CreateServiceTypeAsync("Inspection");
//
//         // CREATE ServiceRecord without ServiceItems
//         var createRequest = new ServiceRecordCreateRequest(
//             Title: "Initial Oil Change",
//             Notes: "First service",
//             Mileage: 15000,
//             ServiceDate: DateTime.UtcNow.AddDays(-5),
//             ManualCost: 120.00m,
//             ServiceTypeId: serviceType1.Id,
//             ServiceItems: new List<ServiceItemCreateRequest>());
//
//         var createResponse = await Client.PostAsJsonAsync(
//             string.Format(ApiV1Definitions.Services.Create, vehicle.Id),
//             createRequest);
//
//         createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
//         var createdDto = await createResponse.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//
//         createdDto.ShouldNotBeNull();
//         createdDto.Id.ShouldNotBe(Guid.Empty);
//         createdDto.Title.ShouldBe("Initial Oil Change");
//         createdDto.Notes.ShouldBe("First service");
//         createdDto.Mileage.ShouldBe(15000);
//         createdDto.TotalCost.ShouldBe(120.00m); // ManualCost
//         createdDto.TypeId.ShouldBe(serviceType1.Id);
//         createdDto.Type.ShouldBe("Oil Change");
//         createdDto.ServiceItems.ShouldBeEmpty();
//
//         // GET ServiceRecord
//         var getResponse = await Client.GetAsync(
//             string.Format(ApiV1Definitions.Services.GetById, vehicle.Id, createdDto.Id));
//
//         getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
//         var getDto = await getResponse.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//
//         getDto.ShouldNotBeNull();
//         getDto.Id.ShouldBe(createdDto.Id);
//         getDto.Title.ShouldBe("Initial Oil Change");
//         getDto.Notes.ShouldBe("First service");
//         getDto.Mileage.ShouldBe(15000);
//         getDto.TotalCost.ShouldBe(120.00m);
//         getDto.TypeId.ShouldBe(serviceType1.Id);
//
//         // UPDATE ServiceRecord
//         var updateRequest = new ServiceRecordUpdateRequest(
//             Title: "Updated Oil Change",
//             Notes: "Updated service notes",
//             Mileage: 16000,
//             ServiceDate: DateTime.UtcNow.AddDays(-3),
//             ManualCost: 150.00m,
//             ServiceTypeId: serviceType2.Id);
//
//         var updateResponse = await Client.PutAsJsonAsync(
//             string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, createdDto.Id),
//             updateRequest);
//
//         updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
//         var updatedDto = await updateResponse.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//
//         updatedDto.ShouldNotBeNull();
//         updatedDto.Id.ShouldBe(createdDto.Id);
//         updatedDto.Title.ShouldBe("Updated Oil Change");
//         updatedDto.Notes.ShouldBe("Updated service notes");
//         updatedDto.Mileage.ShouldBe(16000);
//         updatedDto.TotalCost.ShouldBe(150.00m);
//         updatedDto.TypeId.ShouldBe(serviceType2.Id);
//         updatedDto.Type.ShouldBe("Inspection");
//
//         // GET ServiceRecord after Update
//         var getAfterUpdateResponse = await Client.GetAsync(
//             string.Format(ApiV1Definitions.Services.GetById, vehicle.Id, createdDto.Id));
//
//         getAfterUpdateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
//         var getAfterUpdateDto = await getAfterUpdateResponse.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//
//         getAfterUpdateDto.ShouldNotBeNull();
//         getAfterUpdateDto.Title.ShouldBe("Updated Oil Change");
//         getAfterUpdateDto.Notes.ShouldBe("Updated service notes");
//         getAfterUpdateDto.Mileage.ShouldBe(16000);
//         getAfterUpdateDto.TotalCost.ShouldBe(150.00m);
//         getAfterUpdateDto.TypeId.ShouldBe(serviceType2.Id);
//
//         // DELETE ServiceRecord
//         var deleteResponse = await Client.DeleteAsync(
//             string.Format(ApiV1Definitions.Services.DeleteById, vehicle.Id, createdDto.Id));
//
//         deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
//
//         // TRY GET ServiceRecord after Delete
//         var getAfterDeleteResponse = await Client.GetAsync(
//             string.Format(ApiV1Definitions.Services.GetById, vehicle.Id, createdDto.Id));
//
//         getAfterDeleteResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
//     }
//
//     [Fact]
//     public async Task ServiceRecordLifecycleFlow_WithServiceItems_FullCRUD()
//     {
//         // Setup
//         await CreateAndAuthenticateUser();
//         var vehicle = await CreateVehicleAsync();
//         var serviceType = await CreateServiceTypeAsync("Oil Change");
//
//         // CREATE ServiceRecord with 3 ServiceItems
//         var createRequest = new ServiceRecordCreateRequest(
//             Title: "Complete Oil Change",
//             Notes: "Full service with parts",
//             Mileage: 20000,
//             ServiceDate: DateTime.UtcNow.AddDays(-10),
//             ManualCost: 500.00m, // Should be ignored
//             ServiceTypeId: serviceType.Id,
//             ServiceItems: new List<ServiceItemCreateRequest>
//             {
//                 new("Engine Oil 5W-30", ServiceItemType.Part, 45.00m, 5, "OIL-123", "Premium synthetic"),
//                 new("Oil Filter", ServiceItemType.Part, 15.00m, 1, "FILTER-456", null),
//                 new("Labor", ServiceItemType.Labor, 80.00m, 1, null, "Oil change service")
//             });
//
//         var createResponse = await Client.PostAsJsonAsync(string.Format(ApiV1Definitions.Services.Create, vehicle.Id), createRequest);
//
//         createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
//         var createdDto = await createResponse.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//
//         createdDto.ShouldNotBeNull();
//         createdDto.ServiceItems.Count.ShouldBe(3);
//         createdDto.TotalCost.ShouldBe(320.00m); // 45*5 + 15*1 + 80*1, NOT 500
//         var oilItem = createdDto.ServiceItems.FirstOrDefault(si => si.Name == "Engine Oil 5W-30");
//         oilItem.ShouldNotBeNull();
//         oilItem.Type.ShouldBe(ServiceItemType.Part);
//         oilItem.UnitPrice.ShouldBe(45.00m);
//         oilItem.Quantity.ShouldBe(5);
//         oilItem.PartNumber.ShouldBe("OIL-123");
//
//         var serviceItemIds = createdDto.ServiceItems.Select(si => si.Id).ToList();
//
//         // GET ServiceRecord
//         var getResponse = await Client.GetAsync(string.Format(ApiV1Definitions.Services.GetById, vehicle.Id, createdDto.Id));
//
//         getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
//         var getDto = await getResponse.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//
//         getDto.ShouldNotBeNull();
//         getDto.ServiceItems.Count.ShouldBe(3);
//         getDto.ServiceItems.Any(si => si.Name == "Engine Oil 5W-30").ShouldBeTrue();
//         getDto.ServiceItems.Any(si => si.Name == "Oil Filter").ShouldBeTrue();
//         getDto.ServiceItems.Any(si => si.Name == "Labor").ShouldBeTrue();
//
//         // UPDATE ServiceRecord (only SR fields)
//         var updateRequest = new ServiceRecordUpdateRequest(
//             Title: "Updated Complete Service",
//             Notes: "Updated notes",
//             Mileage: 21000,
//             ServiceDate: DateTime.UtcNow.AddDays(-8),
//             ManualCost: 999.00m, // Should still be ignored
//             ServiceTypeId: serviceType.Id);
//
//         var updateResponse = await Client.PutAsJsonAsync(string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, createdDto.Id), updateRequest);
//
//         updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
//
//         // GET ServiceRecord after Update
//         var getAfterUpdateResponse = await Client.GetAsync(string.Format(ApiV1Definitions.Services.GetById, vehicle.Id, createdDto.Id));
//
//         getAfterUpdateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
//         var getAfterUpdateDto = await getAfterUpdateResponse.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//
//         getAfterUpdateDto.ShouldNotBeNull();
//         getAfterUpdateDto.Title.ShouldBe("Updated Complete Service");
//         getAfterUpdateDto.Mileage.ShouldBe(21000);
//         getAfterUpdateDto.ServiceItems.Count.ShouldBe(3); // Still intact
//         getAfterUpdateDto.TotalCost.ShouldBe(320.00m); // Still from ServiceItems
//         getAfterUpdateDto.ServiceItems.Select(si => si.Id).OrderBy(id => id).ShouldBe(serviceItemIds.OrderBy(id => id));
//
//         // DELETE ServiceRecord
//         var deleteResponse = await Client.DeleteAsync(string.Format(ApiV1Definitions.Services.DeleteById, vehicle.Id, createdDto.Id));
//
//         deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
//
//         // TRY GET ServiceRecord after Delete
//         var getAfterDeleteResponse = await Client.GetAsync(string.Format(ApiV1Definitions.Services.GetById, vehicle.Id, createdDto.Id));
//
//         getAfterDeleteResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
//
//         // VERIFY CASCADE DELETE - ServiceItems should be deleted
//         DbContext.ChangeTracker.Clear();
//         var orphanedItems = DbContext.ServiceItems
//             .Where(si => serviceItemIds.Contains(si.Id))
//             .ToList();
//
//         orphanedItems.Count.ShouldBe(0);
//     }
//
//     [Fact]
//     public async Task ServiceRecordFlow_CreateMinimalThenEnrich_Success()
//     {
//         // Setup
//         await CreateAndAuthenticateUser();
//         var vehicle = await CreateVehicleAsync();
//         var serviceType = await CreateServiceTypeAsync();
//
//         // CREATE minimal ServiceRecord
//         var createRequest = new ServiceRecordCreateRequest(
//             Title: "Basic Service",
//             Notes: null,
//             Mileage: null,
//             ServiceDate: DateTime.UtcNow.AddDays(-1),
//             ManualCost: null,
//             ServiceTypeId: serviceType.Id,
//             ServiceItems: new List<ServiceItemCreateRequest>());
//
//         var createResponse = await Client.PostAsJsonAsync(
//             string.Format(ApiV1Definitions.Services.Create, vehicle.Id),
//             createRequest);
//
//         createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
//         var createdDto = await createResponse.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//
//         createdDto.ShouldNotBeNull();
//         createdDto.Title.ShouldBe("Basic Service");
//         createdDto.Notes.ShouldBeNull();
//         createdDto.Mileage.ShouldBeNull();
//         createdDto.TotalCost.ShouldBe(0m);
//
//         // GET to verify minimal state
//         var getResponse1 = await Client.GetAsync(
//             string.Format(ApiV1Definitions.Services.GetById, vehicle.Id, createdDto.Id));
//
//         var getDto1 = await getResponse1.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//         getDto1.ShouldNotBeNull();
//         getDto1.Notes.ShouldBeNull();
//         getDto1.Mileage.ShouldBeNull();
//         getDto1.TotalCost.ShouldBe(0m);
//
//         // UPDATE - add optional fields (enrich)
//         var enrichRequest = new ServiceRecordUpdateRequest(
//             Title: "Enriched Service",
//             Notes: "Now with notes",
//             Mileage: 30000,
//             ServiceDate: DateTime.UtcNow.AddDays(-1),
//             ManualCost: 200.00m,
//             ServiceTypeId: serviceType.Id);
//
//         var enrichResponse = await Client.PutAsJsonAsync(
//             string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, createdDto.Id),
//             enrichRequest);
//
//         enrichResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
//
//         // GET after enrichment
//         var getResponse2 = await Client.GetAsync(
//             string.Format(ApiV1Definitions.Services.GetById, vehicle.Id, createdDto.Id));
//
//         var getDto2 = await getResponse2.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//         getDto2.ShouldNotBeNull();
//         getDto2.Title.ShouldBe("Enriched Service");
//         getDto2.Notes.ShouldBe("Now with notes");
//         getDto2.Mileage.ShouldBe(30000);
//         getDto2.TotalCost.ShouldBe(200.00m);
//
//         // UPDATE - clear optional fields (back to minimal)
//         var clearRequest = new ServiceRecordUpdateRequest(
//             Title: "Minimal Again",
//             Notes: null,
//             Mileage: null,
//             ServiceDate: DateTime.UtcNow.AddDays(-1),
//             ManualCost: null,
//             ServiceTypeId: serviceType.Id);
//
//         var clearResponse = await Client.PutAsJsonAsync(
//             string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, createdDto.Id),
//             clearRequest);
//
//         clearResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
//
//         // GET after clearing
//         var getResponse3 = await Client.GetAsync(
//             string.Format(ApiV1Definitions.Services.GetById, vehicle.Id, createdDto.Id));
//
//         var getDto3 = await getResponse3.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//         getDto3.ShouldNotBeNull();
//         getDto3.Title.ShouldBe("Minimal Again");
//         getDto3.Notes.ShouldBeNull();
//         getDto3.Mileage.ShouldBeNull();
//         getDto3.TotalCost.ShouldBe(0m);
//     }
//
//     [Fact]
//     public async Task ServiceRecordFlow_UpdateServiceType_Success()
//     {
//         // Setup
//         await CreateAndAuthenticateUser();
//         var vehicle = await CreateVehicleAsync();
//         var oilChangeType = await CreateServiceTypeAsync("Oil Change");
//         var inspectionType = await CreateServiceTypeAsync("Inspection");
//         var repairType = await CreateServiceTypeAsync("Repair");
//
//         // CREATE with "Oil Change" type
//         var createRequest = new ServiceRecordCreateRequest(
//             Title: "Service",
//             Notes: null,
//             Mileage: 10000,
//             ServiceDate: DateTime.UtcNow.AddDays(-5),
//             ManualCost: 100.00m,
//             ServiceTypeId: oilChangeType.Id,
//             ServiceItems: new List<ServiceItemCreateRequest>());
//
//         var createResponse = await Client.PostAsJsonAsync(
//             string.Format(ApiV1Definitions.Services.Create, vehicle.Id),
//             createRequest);
//
//         createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
//         var createdDto = await createResponse.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//
//         createdDto.ShouldNotBeNull();
//         createdDto.TypeId.ShouldBe(oilChangeType.Id);
//         createdDto.Type.ShouldBe("Oil Change");
//
//         // GET to verify initial type
//         var getResponse1 = await Client.GetAsync(
//             string.Format(ApiV1Definitions.Services.GetById, vehicle.Id, createdDto.Id));
//
//         var getDto1 = await getResponse1.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//         getDto1.ShouldNotBeNull();
//         getDto1.Type.ShouldBe("Oil Change");
//         getDto1.TypeId.ShouldBe(oilChangeType.Id);
//
//         // UPDATE to "Inspection" type
//         var updateRequest1 = new ServiceRecordUpdateRequest(
//             Title: "Service",
//             Notes: null,
//             Mileage: 10000,
//             ServiceDate: DateTime.UtcNow.AddDays(-5),
//             ManualCost: 100.00m,
//             ServiceTypeId: inspectionType.Id);
//
//         var updateResponse1 = await Client.PutAsJsonAsync(
//             string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, createdDto.Id),
//             updateRequest1);
//
//         updateResponse1.StatusCode.ShouldBe(HttpStatusCode.OK);
//
//         // GET after first type change
//         var getResponse2 = await Client.GetAsync(
//             string.Format(ApiV1Definitions.Services.GetById, vehicle.Id, createdDto.Id));
//
//         var getDto2 = await getResponse2.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//         getDto2.ShouldNotBeNull();
//         getDto2.Type.ShouldBe("Inspection");
//         getDto2.TypeId.ShouldBe(inspectionType.Id);
//
//         // UPDATE to "Repair" type
//         var updateRequest2 = new ServiceRecordUpdateRequest(
//             Title: "Service",
//             Notes: null,
//             Mileage: 10000,
//             ServiceDate: DateTime.UtcNow.AddDays(-5),
//             ManualCost: 100.00m,
//             ServiceTypeId: repairType.Id);
//
//         var updateResponse2 = await Client.PutAsJsonAsync(
//             string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, createdDto.Id),
//             updateRequest2);
//
//         updateResponse2.StatusCode.ShouldBe(HttpStatusCode.OK);
//
//         // GET after second type change
//         var getResponse3 = await Client.GetAsync(
//             string.Format(ApiV1Definitions.Services.GetById, vehicle.Id, createdDto.Id));
//
//         var getDto3 = await getResponse3.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//         getDto3.ShouldNotBeNull();
//         getDto3.Type.ShouldBe("Repair");
//         getDto3.TypeId.ShouldBe(repairType.Id);
//
//         // UPDATE back to "Oil Change"
//         var updateRequest3 = new ServiceRecordUpdateRequest(
//             Title: "Service",
//             Notes: null,
//             Mileage: 10000,
//             ServiceDate: DateTime.UtcNow.AddDays(-5),
//             ManualCost: 100.00m,
//             ServiceTypeId: oilChangeType.Id);
//
//         var updateResponse3 = await Client.PutAsJsonAsync(
//             string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, createdDto.Id),
//             updateRequest3);
//
//         updateResponse3.StatusCode.ShouldBe(HttpStatusCode.OK);
//
//         // GET after returning to original type
//         var getResponse4 = await Client.GetAsync(
//             string.Format(ApiV1Definitions.Services.GetById, vehicle.Id, createdDto.Id));
//
//         var getDto4 = await getResponse4.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//         getDto4.ShouldNotBeNull();
//         getDto4.Type.ShouldBe("Oil Change");
//         getDto4.TypeId.ShouldBe(oilChangeType.Id);
//     }
//
//     [Fact]
//     public async Task ServiceRecordFlow_CreateMultipleAndList_WithPagination()
//     {
//         // Setup
//         await CreateAndAuthenticateUser();
//         var vehicle = await CreateVehicleAsync();
//         var serviceType = await CreateServiceTypeAsync();
//
//         // CREATE 15 ServiceRecords
//         for (int i = 1; i <= 15; i++)
//         {
//             var createRequest = new ServiceRecordCreateRequest(
//                 Title: $"Service {i}",
//                 Notes: $"Notes for service {i}",
//                 Mileage: 1000 * i,
//                 ServiceDate: DateTime.UtcNow.AddDays(-i),
//                 ManualCost: 100.00m * i,
//                 ServiceTypeId: serviceType.Id,
//                 ServiceItems: new List<ServiceItemCreateRequest>());
//
//             var createResponse = await Client.PostAsJsonAsync(
//                 string.Format(ApiV1Definitions.Services.Create, vehicle.Id),
//                 createRequest);
//
//             createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
//         }
//
//         // GET ALL - First Page (default pageSize=10)
//         var getPage1Response = await Client.GetAsync(
//             string.Format(ApiV1Definitions.Services.GetAll, vehicle.Id));
//
//         getPage1Response.StatusCode.ShouldBe(HttpStatusCode.OK);
//         var page1 = await getPage1Response.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
//
//         page1.ShouldNotBeNull();
//         page1.Items.Count.ShouldBe(10);
//         page1.PageSize.ShouldBe(10);
//         page1.TotalCount.ShouldBe(15);
//         page1.HasNextPage.ShouldBeTrue();
//         page1.HasPreviousPage.ShouldBeFalse();
//
//         // GET ALL - Second Page
//         var getPage2Response = await Client.GetAsync(
//             $"{string.Format(ApiV1Definitions.Services.GetAll, vehicle.Id)}?page=2&pageSize=10");
//
//         getPage2Response.StatusCode.ShouldBe(HttpStatusCode.OK);
//         var page2 = await getPage2Response.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
//
//         page2.ShouldNotBeNull();
//         page2.Items.Count.ShouldBe(5);
//         page2.PageSize.ShouldBe(10);
//         page2.TotalCount.ShouldBe(15);
//         page2.HasNextPage.ShouldBeFalse();
//         page2.HasPreviousPage.ShouldBeTrue();
//
//         // GET ALL - Increased PageSize
//         var getAllResponse = await Client.GetAsync(
//             $"{string.Format(ApiV1Definitions.Services.GetAll, vehicle.Id)}?pageSize=20");
//
//         getAllResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
//         var allPage = await getAllResponse.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
//
//         allPage.ShouldNotBeNull();
//         allPage.Items.Count.ShouldBe(15);
//         allPage.PageSize.ShouldBe(20);
//         allPage.TotalCount.ShouldBe(15);
//         allPage.HasNextPage.ShouldBeFalse();
//         allPage.HasPreviousPage.ShouldBeFalse();
//     }
//
//     [Fact]
//     public async Task ServiceRecordFlow_CascadeDelete_ServiceItemsRemoved()
//     {
//         // Setup
//         await CreateAndAuthenticateUser();
//         var vehicle = await CreateVehicleAsync();
//         var serviceType = await CreateServiceTypeAsync();
//
//         // CREATE ServiceRecord with 5 ServiceItems
//         var createRequest = new ServiceRecordCreateRequest(
//             Title: "Service with many items",
//             Notes: null,
//             Mileage: 25000,
//             ServiceDate: DateTime.UtcNow.AddDays(-7),
//             ManualCost: null,
//             ServiceTypeId: serviceType.Id,
//             ServiceItems: new List<ServiceItemCreateRequest>
//             {
//                 new("Item 1", ServiceItemType.Part, 10.00m, 1, null, null),
//                 new("Item 2", ServiceItemType.Part, 20.00m, 1, null, null),
//                 new("Item 3", ServiceItemType.Labor, 30.00m, 1, null, null),
//                 new("Item 4", ServiceItemType.Part, 40.00m, 1, null, null),
//                 new("Item 5", ServiceItemType.Other, 50.00m, 1, null, null)
//             });
//
//         var createResponse = await Client.PostAsJsonAsync(
//             string.Format(ApiV1Definitions.Services.Create, vehicle.Id),
//             createRequest);
//
//         createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
//         var createdDto = await createResponse.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//
//         createdDto.ShouldNotBeNull();
//         createdDto.ServiceItems.Count.ShouldBe(5);
//
//         var serviceItemIds = createdDto.ServiceItems.Select(si => si.Id).ToList();
//         serviceItemIds.Count.ShouldBe(5);
//
//         // GET ServiceRecord to verify ServiceItems exist
//         var getResponse = await Client.GetAsync(
//             string.Format(ApiV1Definitions.Services.GetById, vehicle.Id, createdDto.Id));
//
//         getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
//         var getDto = await getResponse.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//         getDto.ShouldNotBeNull();
//         getDto.ServiceItems.Count.ShouldBe(5);
//
//         // Query Database - ServiceItems should exist
//         DbContext.ChangeTracker.Clear();
//         var itemsBeforeDelete = DbContext.ServiceItems
//             .Where(si => serviceItemIds.Contains(si.Id))
//             .ToList();
//
//         itemsBeforeDelete.Count.ShouldBe(5);
//
//         // DELETE ServiceRecord
//         var deleteResponse = await Client.DeleteAsync(
//             string.Format(ApiV1Definitions.Services.DeleteById, vehicle.Id, createdDto.Id));
//
//         deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
//
//         // Query Database - ServiceItems should be CASCADE DELETED
//         DbContext.ChangeTracker.Clear();
//         var itemsAfterDelete = DbContext.ServiceItems
//             .Where(si => serviceItemIds.Contains(si.Id))
//             .ToList();
//
//         itemsAfterDelete.Count.ShouldBe(0);
//     }
//
//     [Fact]
//     public async Task ServiceRecordFlow_UpdateWithExistingServiceItems_PreservesItems()
//     {
//         // Setup
//         await CreateAndAuthenticateUser();
//         var vehicle = await CreateVehicleAsync();
//         var serviceType = await CreateServiceTypeAsync();
//
//         // CREATE ServiceRecord with 3 ServiceItems
//         var createRequest = new ServiceRecordCreateRequest(
//             Title: "Original Service",
//             Notes: "Original notes",
//             Mileage: 10000,
//             ServiceDate: DateTime.UtcNow.AddDays(-10),
//             ManualCost: 999.99m, // Should be ignored
//             ServiceTypeId: serviceType.Id,
//             ServiceItems: new List<ServiceItemCreateRequest>
//             {
//                 new("Part A", ServiceItemType.Part, 25.00m, 2, "PART-A", "Part A notes"),
//                 new("Part B", ServiceItemType.Part, 50.00m, 1, "PART-B", "Part B notes"),
//                 new("Labor C", ServiceItemType.Labor, 75.00m, 1, null, "Labor notes")
//             });
//
//         var createResponse = await Client.PostAsJsonAsync(
//             string.Format(ApiV1Definitions.Services.Create, vehicle.Id),
//             createRequest);
//
//         createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
//         var createdDto = await createResponse.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//
//         createdDto.ShouldNotBeNull();
//         createdDto.ServiceItems.Count.ShouldBe(3);
//         createdDto.TotalCost.ShouldBe(175.00m); // 25*2 + 50*1 + 75*1
//
//         // Remember ServiceItems details
//         var originalItems = createdDto.ServiceItems.OrderBy(si => si.Name).ToList();
//         var originalItemIds = originalItems.Select(si => si.Id).ToList();
//         var partA = originalItems.First(si => si.Name == "Part A");
//
//         // GET to verify initial state
//         var getResponse1 = await Client.GetAsync(
//             string.Format(ApiV1Definitions.Services.GetById, vehicle.Id, createdDto.Id));
//
//         var getDto1 = await getResponse1.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//         getDto1.ShouldNotBeNull();
//         getDto1.ServiceItems.Count.ShouldBe(3);
//
//         // UPDATE ServiceRecord (change Title, Notes, Mileage, ManualCost - but NOT ServiceItems)
//         var updateRequest1 = new ServiceRecordUpdateRequest(
//             Title: "Updated Service",
//             Notes: "Updated notes",
//             Mileage: 15000,
//             ServiceDate: DateTime.UtcNow.AddDays(-8),
//             ManualCost: 500.00m, // Should still be ignored
//             ServiceTypeId: serviceType.Id);
//
//         var updateResponse1 = await Client.PutAsJsonAsync(
//             string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, createdDto.Id),
//             updateRequest1);
//
//         updateResponse1.StatusCode.ShouldBe(HttpStatusCode.OK);
//
//         // GET after first update - verify ServiceItems unchanged
//         var getResponse2 = await Client.GetAsync(
//             string.Format(ApiV1Definitions.Services.GetById, vehicle.Id, createdDto.Id));
//
//         var getDto2 = await getResponse2.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//         getDto2.ShouldNotBeNull();
//         getDto2.Title.ShouldBe("Updated Service");
//         getDto2.Notes.ShouldBe("Updated notes");
//         getDto2.Mileage.ShouldBe(15000);
//         getDto2.ServiceItems.Count.ShouldBe(3);
//         getDto2.TotalCost.ShouldBe(175.00m);
//         getDto2.ServiceItems.Select(si => si.Id).OrderBy(id => id).ShouldBe(originalItemIds.OrderBy(id => id));
//
//         var updatedPartA = getDto2.ServiceItems.First(si => si.Name == "Part A");
//         updatedPartA.Id.ShouldBe(partA.Id);
//         updatedPartA.UnitPrice.ShouldBe(25.00m);
//         updatedPartA.Quantity.ShouldBe(2);
//         updatedPartA.PartNumber.ShouldBe("PART-A");
//         updatedPartA.Notes.ShouldBe("Part A notes");
//
//         // UPDATE again with different values
//         var updateRequest2 = new ServiceRecordUpdateRequest(
//             Title: "Another Update",
//             Notes: "Different notes",
//             Mileage: 20000,
//             ServiceDate: DateTime.UtcNow.AddDays(-5),
//             ManualCost: 1000.00m, // Still should be ignored
//             ServiceTypeId: serviceType.Id);
//
//         var updateResponse2 = await Client.PutAsJsonAsync(
//             string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, createdDto.Id),
//             updateRequest2);
//
//         updateResponse2.StatusCode.ShouldBe(HttpStatusCode.OK);
//
//         // GET after second update - ServiceItems still intact
//         var getResponse3 = await Client.GetAsync(
//             string.Format(ApiV1Definitions.Services.GetById, vehicle.Id, createdDto.Id));
//
//         var getDto3 = await getResponse3.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//         getDto3.ShouldNotBeNull();
//         getDto3.Title.ShouldBe("Another Update");
//         getDto3.ServiceItems.Count.ShouldBe(3);
//         getDto3.TotalCost.ShouldBe(175.00m); // Still from ServiceItems
//         getDto3.ServiceItems.Select(si => si.Id).OrderBy(id => id).ShouldBe(originalItemIds.OrderBy(id => id));
//         getDto3.ServiceItems.All(si => originalItems.Any(oi =>
//             oi.Id == si.Id &&
//             oi.Name == si.Name &&
//             oi.UnitPrice == si.UnitPrice &&
//             oi.Quantity == si.Quantity)).ShouldBeTrue();
//     }
//
//     [Fact]
//     public async Task ServiceRecordFlow_InvalidScenarios_BadRequest()
//     {
//         // Setup
//         await CreateAndAuthenticateUser();
//         var vehicle = await CreateVehicleAsync();
//         var serviceType = await CreateServiceTypeAsync();
//
//         // CREATE with empty Title - should fail
//         var createRequest1 = new ServiceRecordCreateRequest(
//             Title: "",
//             Notes: null,
//             Mileage: null,
//             ServiceDate: DateTime.UtcNow,
//             ManualCost: null,
//             ServiceTypeId: serviceType.Id,
//             ServiceItems: new List<ServiceItemCreateRequest>());
//
//         var createResponse1 = await Client.PostAsJsonAsync(string.Format(ApiV1Definitions.Services.Create, vehicle.Id), createRequest1);
//
//         createResponse1.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
//
//         // CREATE with future date - should fail
//         var createRequest2 = new ServiceRecordCreateRequest(
//             Title: "Future Service",
//             Notes: null,
//             Mileage: null,
//             ServiceDate: DateTime.UtcNow.AddDays(10),
//             ManualCost: null,
//             ServiceTypeId: serviceType.Id,
//             ServiceItems: new List<ServiceItemCreateRequest>());
//
//         var createResponse2 = await Client.PostAsJsonAsync(string.Format(ApiV1Definitions.Services.Create, vehicle.Id), createRequest2);
//
//         createResponse2.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
//
//         // CREATE with negative mileage - should fail
//         var createRequest3 = new ServiceRecordCreateRequest(
//             Title: "Negative Mileage",
//             Notes: null,
//             Mileage: -100,
//             ServiceDate: DateTime.UtcNow,
//             ManualCost: null,
//             ServiceTypeId: serviceType.Id,
//             ServiceItems: new List<ServiceItemCreateRequest>());
//
//         var createResponse3 = await Client.PostAsJsonAsync(string.Format(ApiV1Definitions.Services.Create, vehicle.Id), createRequest3);
//
//         createResponse3.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
//
//         // CREATE valid ServiceRecord for update tests
//         var validCreateRequest = new ServiceRecordCreateRequest(
//             Title: "Valid Service",
//             Notes: "Valid notes",
//             Mileage: 10000,
//             ServiceDate: DateTime.UtcNow.AddDays(-1),
//             ManualCost: 100.00m,
//             ServiceTypeId: serviceType.Id,
//             ServiceItems: new List<ServiceItemCreateRequest>());
//
//         var validCreateResponse = await Client.PostAsJsonAsync(string.Format(ApiV1Definitions.Services.Create, vehicle.Id), validCreateRequest);
//
//         validCreateResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
//         var validDto = await validCreateResponse.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//         validDto.ShouldNotBeNull();
//
//         // UPDATE with empty Title - should fail
//         var updateRequest1 = new ServiceRecordUpdateRequest(
//             Title: "",
//             Notes: "Notes",
//             Mileage: 10000,
//             ServiceDate: DateTime.UtcNow.AddDays(-1),
//             ManualCost: 100.00m,
//             ServiceTypeId: serviceType.Id);
//
//         var updateResponse1 = await Client.PutAsJsonAsync(string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, validDto.Id), updateRequest1);
//
//         updateResponse1.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
//
//         // UPDATE with negative ManualCost - should fail
//         var updateRequest2 = new ServiceRecordUpdateRequest(
//             Title: "Valid Title",
//             Notes: null,
//             Mileage: 10000,
//             ServiceDate: DateTime.UtcNow.AddDays(-1),
//             ManualCost: -50.00m,
//             ServiceTypeId: serviceType.Id);
//
//         var updateResponse2 = await Client.PutAsJsonAsync(string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, validDto.Id), updateRequest2);
//
//         updateResponse2.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
//
//         // GET ServiceRecord - should still have original data (failed updates didn't modify it)
//         var getResponse = await Client.GetAsync(string.Format(ApiV1Definitions.Services.GetById, vehicle.Id, validDto.Id));
//
//         getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
//         var getDto = await getResponse.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//
//         getDto.ShouldNotBeNull();
//         getDto.Title.ShouldBe("Valid Service");
//         getDto.Notes.ShouldBe("Valid notes");
//         getDto.Mileage.ShouldBe(10000);
//         getDto.TotalCost.ShouldBe(100.00m);
//     }
//
//     [Fact]
//     public async Task ServiceRecordFlow_MultipleVehicles_IsolationCheck()
//     {
//         // Setup
//         await CreateAndAuthenticateUser();
//         var vehicle1 = await CreateVehicleAsync();
//         var vehicle2 = await CreateVehicleAsync("Honda", "Civic");
//         var serviceType = await CreateServiceTypeAsync();
//
//         // CREATE ServiceRecord for Vehicle1
//         var createRequest1 = new ServiceRecordCreateRequest(
//             Title: "Service for Vehicle 1",
//             Notes: "Vehicle 1 notes",
//             Mileage: 10000,
//             ServiceDate: DateTime.UtcNow.AddDays(-5),
//             ManualCost: 100.00m,
//             ServiceTypeId: serviceType.Id,
//             ServiceItems: new List<ServiceItemCreateRequest>());
//
//         var createResponse1 = await Client.PostAsJsonAsync(
//             string.Format(ApiV1Definitions.Services.Create, vehicle1.Id),
//             createRequest1);
//
//         createResponse1.StatusCode.ShouldBe(HttpStatusCode.Created);
//         var sr1 = await createResponse1.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//         sr1.ShouldNotBeNull();
//
//         // CREATE ServiceRecord for Vehicle2
//         var createRequest2 = new ServiceRecordCreateRequest(
//             Title: "Service for Vehicle 2",
//             Notes: "Vehicle 2 notes",
//             Mileage: 20000,
//             ServiceDate: DateTime.UtcNow.AddDays(-3),
//             ManualCost: 200.00m,
//             ServiceTypeId: serviceType.Id,
//             ServiceItems: new List<ServiceItemCreateRequest>());
//
//         var createResponse2 = await Client.PostAsJsonAsync(
//             string.Format(ApiV1Definitions.Services.Create, vehicle2.Id),
//             createRequest2);
//
//         createResponse2.StatusCode.ShouldBe(HttpStatusCode.Created);
//         var sr2 = await createResponse2.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//         sr2.ShouldNotBeNull();
//
//         // GET SR1 via Vehicle1 path - should succeed
//         var getSr1Response = await Client.GetAsync(
//             string.Format(ApiV1Definitions.Services.GetById, vehicle1.Id, sr1.Id));
//
//         getSr1Response.StatusCode.ShouldBe(HttpStatusCode.OK);
//         var getSr1Dto = await getSr1Response.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//         getSr1Dto.ShouldNotBeNull();
//         getSr1Dto.Title.ShouldBe("Service for Vehicle 1");
//
//         // TRY GET SR1 via Vehicle2 path (wrong vehicle) - should fail
//         var getSr1WrongVehicleResponse = await Client.GetAsync(
//             string.Format(ApiV1Definitions.Services.GetById, vehicle2.Id, sr1.Id));
//
//         getSr1WrongVehicleResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
//
//         // GET SR2 via Vehicle2 path - should succeed
//         var getSr2Response = await Client.GetAsync(
//             string.Format(ApiV1Definitions.Services.GetById, vehicle2.Id, sr2.Id));
//
//         getSr2Response.StatusCode.ShouldBe(HttpStatusCode.OK);
//         var getSr2Dto = await getSr2Response.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//         getSr2Dto.ShouldNotBeNull();
//         getSr2Dto.Title.ShouldBe("Service for Vehicle 2");
//
//         // TRY GET SR2 via Vehicle1 path (wrong vehicle) - should fail
//         var getSr2WrongVehicleResponse = await Client.GetAsync(
//             string.Format(ApiV1Definitions.Services.GetById, vehicle1.Id, sr2.Id));
//
//         getSr2WrongVehicleResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
//
//         // DELETE SR1
//         var deleteSr1Response = await Client.DeleteAsync(
//             string.Format(ApiV1Definitions.Services.DeleteById, vehicle1.Id, sr1.Id));
//
//         deleteSr1Response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
//
//         // GET SR2 - should still exist (unaffected by SR1 deletion)
//         var getSr2AfterDeleteResponse = await Client.GetAsync(
//             string.Format(ApiV1Definitions.Services.GetById, vehicle2.Id, sr2.Id));
//
//         getSr2AfterDeleteResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
//         var getSr2AfterDeleteDto = await getSr2AfterDeleteResponse.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//         getSr2AfterDeleteDto.ShouldNotBeNull();
//         getSr2AfterDeleteDto.Title.ShouldBe("Service for Vehicle 2");
//         getSr2AfterDeleteDto.TotalCost.ShouldBe(200.00m);
//     }
//
//     [Fact]
//     public async Task ServiceRecordFlow_UnauthorizedAccess_SecurityCheck()
//     {
//         // Setup User1
//         var user1 = await CreateUserAsync("user1@garagge.app");
//         var loginUser1Response = await LoginUser(user1.Email, "Password123");
//         Authenticate(loginUser1Response.Response.AccessToken);
//
//         var serviceType = await CreateServiceTypeAsync();
//         var vehicle1 = await CreateVehicleAsync(user1, "User1 Car", "Model1"); // Use user object
//
//         // CREATE ServiceRecord as User1
//         var createRequest = new ServiceRecordCreateRequest(
//             Title: "User1 Service",
//             Notes: "Private service",
//             Mileage: 5000,
//             ServiceDate: DateTime.UtcNow.AddDays(-2),
//             ManualCost: 150.00m,
//             ServiceTypeId: serviceType.Id,
//             ServiceItems: new List<ServiceItemCreateRequest>());
//
//         var createResponse = await Client.PostAsJsonAsync(
//             string.Format(ApiV1Definitions.Services.Create, vehicle1.Id),
//             createRequest);
//
//         createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
//         var sr = await createResponse.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//         sr.ShouldNotBeNull();
//
//         // GET as User1 - should succeed
//         var getAsUser1Response = await Client.GetAsync(
//             string.Format(ApiV1Definitions.Services.GetById, vehicle1.Id, sr.Id));
//
//         getAsUser1Response.StatusCode.ShouldBe(HttpStatusCode.OK);
//
//         // Setup User2
//         var user2 = await CreateUserAsync("user2@garagge.app", "Password456");
//         var loginUser2Response = await LoginUser(user2.Email, "Password456");
//         Authenticate(loginUser2Response.Response.AccessToken);
//
//         // TRY GET as User2 (using User1's vehicle/SR) - should fail
//         var getAsUser2Response = await Client.GetAsync(
//             string.Format(ApiV1Definitions.Services.GetById, vehicle1.Id, sr.Id));
//
//         getAsUser2Response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
//
//         // TRY UPDATE as User2 - should fail
//         var updateRequest = new ServiceRecordUpdateRequest(
//             Title: "Hacker Update",
//             Notes: "Trying to modify",
//             Mileage: 99999,
//             ServiceDate: DateTime.UtcNow,
//             ManualCost: 999.00m,
//             ServiceTypeId: serviceType.Id);
//
//         var updateAsUser2Response = await Client.PutAsJsonAsync(
//             string.Format(ApiV1Definitions.Services.UpdateById, vehicle1.Id, sr.Id),
//             updateRequest);
//
//         updateAsUser2Response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
//
//         // TRY DELETE as User2 - should fail
//         var deleteAsUser2Response = await Client.DeleteAsync(
//             string.Format(ApiV1Definitions.Services.DeleteById, vehicle1.Id, sr.Id));
//
//         deleteAsUser2Response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
//
//         // Authenticate back as User1
//         Authenticate(loginUser1Response.Response.AccessToken);
//
//         // GET as User1 again - data should be intact
//         var getFinalResponse = await Client.GetAsync(
//             string.Format(ApiV1Definitions.Services.GetById, vehicle1.Id, sr.Id));
//
//         getFinalResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
//         var finalDto = await getFinalResponse.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//
//         finalDto.ShouldNotBeNull();
//         finalDto.Title.ShouldBe("User1 Service");
//         finalDto.Notes.ShouldBe("Private service");
//         finalDto.Mileage.ShouldBe(5000);
//         finalDto.TotalCost.ShouldBe(150.00m);
//     }
//
//     [Fact]
//     public async Task ServiceRecordFlow_FilteringAndSorting_Success()
//     {
//         // Setup
//         await CreateAndAuthenticateUser();
//         var vehicle = await CreateVehicleAsync();
//         var oilChangeType = await CreateServiceTypeAsync("Oil Change");
//         var inspectionType = await CreateServiceTypeAsync("Inspection");
//
//         // CREATE multiple ServiceRecords with different data
//         var records = new[]
//         {
//             new { Title = "Alpha Service", Type = oilChangeType.Id, Date = DateTime.UtcNow.AddDays(-10), Mileage = 5000, Cost = 100.00m },
//             new { Title = "Beta Oil Service", Type = inspectionType.Id, Date = DateTime.UtcNow.AddDays(-8), Mileage = 10000, Cost = 200.00m },
//             new { Title = "Charlie Inspection Check", Type = oilChangeType.Id, Date = DateTime.UtcNow.AddDays(-6), Mileage = 15000, Cost = 150.00m },
//             new { Title = "Delta Inspection", Type = inspectionType.Id, Date = DateTime.UtcNow.AddDays(-4), Mileage = 20000, Cost = 300.00m },
//             new { Title = "Echo Oil Change", Type = oilChangeType.Id, Date = DateTime.UtcNow.AddDays(-2), Mileage = 25000, Cost = 250.00m }
//         };
//
//         foreach (var record in records)
//         {
//             var createRequest = new ServiceRecordCreateRequest(
//                 Title: record.Title,
//                 Notes: null,
//                 Mileage: record.Mileage,
//                 ServiceDate: record.Date,
//                 ManualCost: record.Cost,
//                 ServiceTypeId: record.Type,
//                 ServiceItems: new List<ServiceItemCreateRequest>());
//
//             var createResponse = await Client.PostAsJsonAsync(
//                 string.Format(ApiV1Definitions.Services.Create, vehicle.Id),
//                 createRequest);
//
//             createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
//         }
//
//         // FILTER by searchTerm "Oil" - should return 2 records (Beta Oil Service, Echo Oil Change)
//         var filterByOilResponse = await Client.GetAsync(
//             $"{string.Format(ApiV1Definitions.Services.GetAll, vehicle.Id)}?searchTerm=Oil");
//
//         filterByOilResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
//         var filterByOilResult = await filterByOilResponse.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
//
//         filterByOilResult.ShouldNotBeNull();
//         filterByOilResult.Items.Count.ShouldBe(2);
//         filterByOilResult.Items.All(sr => sr.Title.Contains("Oil")).ShouldBeTrue();
//
//         // FILTER by searchTerm "Inspection" - should return 2 records
//         var filterByInspectionResponse = await Client.GetAsync(
//             $"{string.Format(ApiV1Definitions.Services.GetAll, vehicle.Id)}?searchTerm=Inspection");
//
//         filterByInspectionResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
//         var filterByInspectionResult = await filterByInspectionResponse.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
//
//         filterByInspectionResult.ShouldNotBeNull();
//         filterByInspectionResult.Items.Count.ShouldBe(2);
//         filterByInspectionResult.Items.All(sr => sr.Title.Contains("Inspection")).ShouldBeTrue();
//
//         // SORT by ServiceDate DESC (default) - should return Echo first
//         var sortDefaultResponse = await Client.GetAsync(
//             string.Format(ApiV1Definitions.Services.GetAll, vehicle.Id));
//
//         sortDefaultResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
//         var sortDefaultResult = await sortDefaultResponse.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
//
//         sortDefaultResult.ShouldNotBeNull();
//         sortDefaultResult.Items.Count.ShouldBe(5);
//         sortDefaultResult.Items.First().Title.ShouldBe("Echo Oil Change"); // Most recent
//         sortDefaultResult.Items.Last().Title.ShouldBe("Alpha Service"); // Oldest
//
//         // SORT by ServiceDate ASC - should return Alpha first
//         var sortAscResponse = await Client.GetAsync(
//             $"{string.Format(ApiV1Definitions.Services.GetAll, vehicle.Id)}?sortBy=ServiceDate&sortDescending=false");
//
//         sortAscResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
//         var sortAscResult = await sortAscResponse.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
//
//         sortAscResult.ShouldNotBeNull();
//         sortAscResult.Items.First().Title.ShouldBe("Alpha Service"); // Oldest
//         sortAscResult.Items.Last().Title.ShouldBe("Echo Oil Change"); // Most recent
//
//         // SORT by Mileage DESC - should return Echo first (25000)
//         var sortByMileageDescResponse = await Client.GetAsync(
//             $"{string.Format(ApiV1Definitions.Services.GetAll, vehicle.Id)}?sortBy=Mileage&sortDescending=true");
//
//         sortByMileageDescResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
//         var sortByMileageDescResult = await sortByMileageDescResponse.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
//
//         sortByMileageDescResult.ShouldNotBeNull();
//         sortByMileageDescResult.Items.First().Mileage.ShouldBe(25000);
//         sortByMileageDescResult.Items.Last().Mileage.ShouldBe(5000);
//
//         // SORT by TotalCost DESC - should return Delta first (300.00)
//         var sortByCostDescResponse = await Client.GetAsync(
//             $"{string.Format(ApiV1Definitions.Services.GetAll, vehicle.Id)}?sortBy=TotalCost&sortDescending=true");
//
//         sortByCostDescResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
//         var sortByCostDescResult = await sortByCostDescResponse.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
//
//         sortByCostDescResult.ShouldNotBeNull();
//         sortByCostDescResult.Items.First().Title.ShouldBe("Delta Inspection"); // 300.00
//         sortByCostDescResult.Items.Last().Title.ShouldBe("Alpha Service"); // 100.00
//     }
//
//     [Fact]
//     public async Task ServiceRecordFlow_InvalidGuidFormat_ReturnsNotFound()
//     {
//         // Setup
//         await CreateAndAuthenticateUser();
//         var vehicle = await CreateVehicleAsync();
//         var serviceType = await CreateServiceTypeAsync();
//
//         // CREATE valid ServiceRecord
//         var createRequest = new ServiceRecordCreateRequest(
//             Title: "Valid Service",
//             Notes: null,
//             Mileage: 10000,
//             ServiceDate: DateTime.UtcNow.AddDays(-1),
//             ManualCost: 100.00m,
//             ServiceTypeId: serviceType.Id,
//             ServiceItems: new List<ServiceItemCreateRequest>());
//
//         var createResponse = await Client.PostAsJsonAsync(
//             string.Format(ApiV1Definitions.Services.Create, vehicle.Id),
//             createRequest);
//
//         createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
//         var createdDto = await createResponse.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//         createdDto.ShouldNotBeNull();
//
//         // TRY GET with invalid ServiceRecordId format - should return NotFound (routing fails)
//         var getInvalidSrIdResponse = await Client.GetAsync(
//             $"/api/vehicles/{vehicle.Id}/service-records/invalid-guid-format");
//
//         getInvalidSrIdResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
//
//         // TRY GET with invalid VehicleId format - should return NotFound (routing fails)
//         var getInvalidVehicleIdResponse = await Client.GetAsync(
//             $"/api/vehicles/not-a-guid/service-records/{createdDto.Id}");
//
//         getInvalidVehicleIdResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
//
//         // TRY UPDATE with invalid ServiceRecordId format - should return NotFound
//         var updateRequest = new ServiceRecordUpdateRequest(
//             Title: "Updated",
//             Notes: null,
//             Mileage: 10000,
//             ServiceDate: DateTime.UtcNow,
//             ManualCost: 100.00m,
//             ServiceTypeId: serviceType.Id);
//
//         var updateInvalidResponse = await Client.PutAsJsonAsync(
//             $"/api/vehicles/{vehicle.Id}/service-records/invalid-guid",
//             updateRequest);
//
//         updateInvalidResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
//
//         // TRY DELETE with invalid ServiceRecordId format - should return NotFound
//         var deleteInvalidResponse = await Client.DeleteAsync(
//             $"/api/vehicles/{vehicle.Id}/service-records/bad-guid-format");
//
//         deleteInvalidResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
//
//         // VERIFY original ServiceRecord is still intact
//         var getValidResponse = await Client.GetAsync(
//             string.Format(ApiV1Definitions.Services.GetById, vehicle.Id, createdDto.Id));
//
//         getValidResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
//         var validDto = await getValidResponse.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//         validDto.ShouldNotBeNull();
//         validDto.Title.ShouldBe("Valid Service");
//     }
//
//     [Fact]
//     public async Task ServiceRecordFlow_UpdateWithNonExistentServiceType_ReturnsNotFound()
//     {
//         // Setup
//         await CreateAndAuthenticateUser();
//         var vehicle = await CreateVehicleAsync();
//         var serviceType = await CreateServiceTypeAsync("Original Type");
//
//         // CREATE ServiceRecord
//         var createRequest = new ServiceRecordCreateRequest(
//             Title: "Original Service",
//             Notes: "Original notes",
//             Mileage: 10000,
//             ServiceDate: DateTime.UtcNow.AddDays(-5),
//             ManualCost: 100.00m,
//             ServiceTypeId: serviceType.Id,
//             ServiceItems: new List<ServiceItemCreateRequest>());
//
//         var createResponse = await Client.PostAsJsonAsync(
//             string.Format(ApiV1Definitions.Services.Create, vehicle.Id),
//             createRequest);
//
//         createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
//         var createdDto = await createResponse.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//         createdDto.ShouldNotBeNull();
//
//         // GET to verify initial state
//         var getResponse1 = await Client.GetAsync(
//             string.Format(ApiV1Definitions.Services.GetById, vehicle.Id, createdDto.Id));
//
//         getResponse1.StatusCode.ShouldBe(HttpStatusCode.OK);
//         var getDto1 = await getResponse1.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//         getDto1.ShouldNotBeNull();
//         getDto1.TypeId.ShouldBe(serviceType.Id);
//         getDto1.Type.ShouldBe("Original Type");
//
//         // TRY UPDATE with non-existent ServiceTypeId - should fail
//         var nonExistentServiceTypeId = Guid.NewGuid();
//         var updateRequest = new ServiceRecordUpdateRequest(
//             Title: "Updated Service",
//             Notes: "Updated notes",
//             Mileage: 15000,
//             ServiceDate: DateTime.UtcNow.AddDays(-3),
//             ManualCost: 150.00m,
//             ServiceTypeId: nonExistentServiceTypeId);
//
//         var updateResponse = await Client.PutAsJsonAsync(
//             string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, createdDto.Id),
//             updateRequest);
//
//         updateResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
//
//         // GET after failed update - data should remain unchanged
//         var getResponse2 = await Client.GetAsync(
//             string.Format(ApiV1Definitions.Services.GetById, vehicle.Id, createdDto.Id));
//
//         getResponse2.StatusCode.ShouldBe(HttpStatusCode.OK);
//         var getDto2 = await getResponse2.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//         getDto2.ShouldNotBeNull();
//         getDto2.Title.ShouldBe("Original Service"); // Unchanged
//         getDto2.Notes.ShouldBe("Original notes"); // Unchanged
//         getDto2.Mileage.ShouldBe(10000); // Unchanged
//         getDto2.TotalCost.ShouldBe(100.00m); // Unchanged
//         getDto2.TypeId.ShouldBe(serviceType.Id); // Still original type
//         getDto2.Type.ShouldBe("Original Type");
//
//         // TRY UPDATE with Guid.Empty for ServiceTypeId - should fail
//         var updateRequestEmpty = new ServiceRecordUpdateRequest(
//             Title: "Another Update",
//             Notes: null,
//             Mileage: 10000,
//             ServiceDate: DateTime.UtcNow,
//             ManualCost: 100.00m,
//             ServiceTypeId: Guid.Empty);
//
//         var updateResponseEmpty = await Client.PutAsJsonAsync(
//             string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, createdDto.Id),
//             updateRequestEmpty);
//
//         updateResponseEmpty.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
//     }
//
//     [Fact]
//     public async Task ServiceRecordFlow_TotalCostCalculation_EdgeCases()
//     {
//         // Setup
//         await CreateAndAuthenticateUser();
//         var vehicle = await CreateVehicleAsync();
//         var serviceType = await CreateServiceTypeAsync();
//
//         // CASE 1: ManualCost = 0, no ServiceItems -> TotalCost = 0
//         var createRequest1 = new ServiceRecordCreateRequest(
//             Title: "Zero Cost Service",
//             Notes: "Free under warranty",
//             Mileage: 5000,
//             ServiceDate: DateTime.UtcNow.AddDays(-5),
//             ManualCost: 0m,
//             ServiceTypeId: serviceType.Id,
//             ServiceItems: new List<ServiceItemCreateRequest>());
//
//         var createResponse1 = await Client.PostAsJsonAsync(
//             string.Format(ApiV1Definitions.Services.Create, vehicle.Id),
//             createRequest1);
//
//         createResponse1.StatusCode.ShouldBe(HttpStatusCode.Created);
//         var dto1 = await createResponse1.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//         dto1.ShouldNotBeNull();
//         dto1.TotalCost.ShouldBe(0m);
//
//         // CASE 2: ManualCost = null, no ServiceItems -> TotalCost = 0
//         var createRequest2 = new ServiceRecordCreateRequest(
//             Title: "No Cost Provided",
//             Notes: null,
//             Mileage: 10000,
//             ServiceDate: DateTime.UtcNow.AddDays(-4),
//             ManualCost: null,
//             ServiceTypeId: serviceType.Id,
//             ServiceItems: new List<ServiceItemCreateRequest>());
//
//         var createResponse2 = await Client.PostAsJsonAsync(
//             string.Format(ApiV1Definitions.Services.Create, vehicle.Id),
//             createRequest2);
//
//         createResponse2.StatusCode.ShouldBe(HttpStatusCode.Created);
//         var dto2 = await createResponse2.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//         dto2.ShouldNotBeNull();
//         dto2.TotalCost.ShouldBe(0m);
//
//         // CASE 3: ServiceItems with zero quantity -> TotalCost = 0
//         var createRequest3 = new ServiceRecordCreateRequest(
//             Title: "Quote Only",
//             Notes: "Parts quoted, not purchased",
//             Mileage: 15000,
//             ServiceDate: DateTime.UtcNow.AddDays(-3),
//             ManualCost: 500.00m, // Should be ignored
//             ServiceTypeId: serviceType.Id,
//             ServiceItems: new List<ServiceItemCreateRequest>
//             {
//                 new("Brake Pads", ServiceItemType.Part, 100.00m, 0, "BP-123", "Quote only"),
//                 new("Oil Filter", ServiceItemType.Part, 15.00m, 0, "OF-456", "Quote only")
//             });
//
//         var createResponse3 = await Client.PostAsJsonAsync(
//             string.Format(ApiV1Definitions.Services.Create, vehicle.Id),
//             createRequest3);
//
//         createResponse3.StatusCode.ShouldBe(HttpStatusCode.Created);
//         var dto3 = await createResponse3.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//         dto3.ShouldNotBeNull();
//         dto3.TotalCost.ShouldBe(0m); // Zero quantity means TotalCost = 0
//         dto3.ServiceItems.Count.ShouldBe(2);
//         dto3.ServiceItems.All(si => si.TotalPrice == 0m).ShouldBeTrue();
//
//         // CASE 4: Mixed - some items with quantity, some with zero -> calculate only non-zero
//         var createRequest4 = new ServiceRecordCreateRequest(
//             Title: "Partial Service",
//             Notes: "Some parts installed, some quoted",
//             Mileage: 20000,
//             ServiceDate: DateTime.UtcNow.AddDays(-2),
//             ManualCost: 999.99m, // Should be ignored
//             ServiceTypeId: serviceType.Id,
//             ServiceItems: new List<ServiceItemCreateRequest>
//             {
//                 new("Installed Part", ServiceItemType.Part, 50.00m, 2, null, null), // 100.00
//                 new("Quote Part", ServiceItemType.Part, 100.00m, 0, null, null), // 0.00
//                 new("Labor", ServiceItemType.Labor, 80.00m, 1, null, null) // 80.00
//             });
//
//         var createResponse4 = await Client.PostAsJsonAsync(
//             string.Format(ApiV1Definitions.Services.Create, vehicle.Id),
//             createRequest4);
//
//         createResponse4.StatusCode.ShouldBe(HttpStatusCode.Created);
//         var dto4 = await createResponse4.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//         dto4.ShouldNotBeNull();
//         dto4.TotalCost.ShouldBe(180.00m); // 50*2 + 0 + 80*1
//         dto4.ServiceItems.Count.ShouldBe(3);
//
//         // CASE 5: UPDATE - change from ManualCost to ServiceItems
//         var createRequest5 = new ServiceRecordCreateRequest(
//             Title: "Manual Cost Service",
//             Notes: null,
//             Mileage: 25000,
//             ServiceDate: DateTime.UtcNow.AddDays(-1),
//             ManualCost: 200.00m,
//             ServiceTypeId: serviceType.Id,
//             ServiceItems: new List<ServiceItemCreateRequest>());
//
//         var createResponse5 = await Client.PostAsJsonAsync(
//             string.Format(ApiV1Definitions.Services.Create, vehicle.Id),
//             createRequest5);
//
//         createResponse5.StatusCode.ShouldBe(HttpStatusCode.Created);
//         var dto5 = await createResponse5.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//         dto5.ShouldNotBeNull();
//         dto5.TotalCost.ShouldBe(200.00m); // ManualCost used
//
//         // Now UPDATE to clear ManualCost -> TotalCost should become 0
//         var updateRequest5 = new ServiceRecordUpdateRequest(
//             Title: "Manual Cost Service",
//             Notes: null,
//             Mileage: 25000,
//             ServiceDate: DateTime.UtcNow.AddDays(-1),
//             ManualCost: null, // Clear cost
//             ServiceTypeId: serviceType.Id);
//
//         var updateResponse5 = await Client.PutAsJsonAsync(
//             string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, dto5.Id),
//             updateRequest5);
//
//         updateResponse5.StatusCode.ShouldBe(HttpStatusCode.OK);
//
//         var getUpdatedResponse5 = await Client.GetAsync(
//             string.Format(ApiV1Definitions.Services.GetById, vehicle.Id, dto5.Id));
//
//         var updatedDto5 = await getUpdatedResponse5.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
//         updatedDto5.ShouldNotBeNull();
//         updatedDto5.TotalCost.ShouldBe(0m); // Should be 0 when ManualCost is null and no ServiceItems
//     }
// }