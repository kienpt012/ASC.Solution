using ASC.Business.Interfaces;
using ASC.DataAccess.Interfaces;
using ASC.Model.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASC.Business
{
    public class MasterDataOperations : IMasterDataOperations
    {
        private readonly IUnitOfWork _unitOfWork;
        public MasterDataOperations(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<MasterDataKey>> GetAllMasterKeysAsync()
        {
            var masterKeys = await _unitOfWork.Repository<MasterDataKey>().FindAllAsync();
            return masterKeys.ToList();
        }

        public async Task<List<MasterDataKey>> GetMaserKeyByNameAsync(string name)
        {
            var masterKeys = await _unitOfWork.Repository<MasterDataKey>().FindAllByPartitionKeyAsync(name);
            return masterKeys.ToList();
        }

        public async Task<bool> InsertMasterKeyAsync(MasterDataKey key)
        {
            using (_unitOfWork)
            {
                await _unitOfWork.Repository<MasterDataKey>().AddAsync(key);
                _unitOfWork.CommitTransaction();
                return true;
            }
        }

        public async Task<List<MasterDataValue>> GetAllMasterValuesByKeyAsync(string key)
        {
            var masterKeys = await _unitOfWork.Repository<MasterDataValue>().FindAllByPartitionKeyAsync(key);
            return masterKeys.ToList();
        }

        public async Task<MasterDataValue> GetMasterValueByNameAsync(string key, string name)
        {
            var masterValues = await _unitOfWork.Repository<MasterDataValue>().FindAsync(key, name);
            return masterValues;
        }

        public async Task<bool> InsertMasterValueAsync(MasterDataValue value)
        {
            using (_unitOfWork)
            {
                await _unitOfWork.Repository<MasterDataValue>().AddAsync(value);
                _unitOfWork.CommitTransaction();
                return true;
            }
        }

        public async Task<bool> UpdateMasterKeyAsync(string orginalPartitionKey, MasterDataKey key)
        {
            using (_unitOfWork)
            {
                var masterKey = await _unitOfWork.Repository<MasterDataKey>().FindAsync(orginalPartitionKey, key.RowKey);
                masterKey.IsActive = key.IsActive;
                masterKey.IsDeleted = key.IsDeleted;
                masterKey.Name = key.Name;
                masterKey.UpdatedBy = masterKey.UpdatedBy;
                _unitOfWork.Repository<MasterDataKey>().Update(masterKey);
                _unitOfWork.CommitTransaction();
                return true;
            }
        }

        public async Task<bool> UpdateMasterValueAsync(string originalPartitionKey, string originalRowKey, MasterDataValue value)
        {
            using (_unitOfWork)
            {
                var masterValue = await _unitOfWork.Repository<MasterDataValue>().FindAsync(originalPartitionKey, originalRowKey);
                masterValue.IsActive = value.IsActive;
                masterValue.IsDeleted = value.IsDeleted;
                masterValue.Name = value.Name;

                _unitOfWork.Repository<MasterDataValue>().Update(masterValue);
                _unitOfWork.CommitTransaction();
                return true;
            }
        }

        public async Task<bool> UploadBulkMasterData(List<MasterDataValue> values)
        {
            using (_unitOfWork)
            {
                // Tạo một danh sách để theo dõi các PartitionKey đã xử lý trong lần upload này
                HashSet<string> processedPartitionKeys = new HashSet<string>();

                // Lấy tất cả MasterKeys hiện có từ database
                var existingMasterKeys = await GetAllMasterKeysAsync();
                HashSet<string> existingPartitionKeys = new HashSet<string>(
                    existingMasterKeys.Select(mk => mk.PartitionKey));

                foreach (var value in values)
                {
                    // Kiểm tra xem PartitionKey đã tồn tại trong DB hoặc đã được xử lý trong lần upload này chưa
                    if (!existingPartitionKeys.Contains(value.PartitionKey) &&
                        !processedPartitionKeys.Contains(value.PartitionKey))
                    {
                        // Thêm MasterKey mới
                        await _unitOfWork.Repository<MasterDataKey>().AddAsync(new MasterDataKey()
                        {
                            Name = value.PartitionKey,
                            RowKey = Guid.NewGuid().ToString(),
                            PartitionKey = value.PartitionKey
                        });

                        // Đánh dấu PartitionKey này đã được xử lý
                        processedPartitionKeys.Add(value.PartitionKey);
                    }

                    // Xử lý MasterValue
                    var masterValuesByKey = await GetAllMasterValuesByKeyAsync(value.PartitionKey);
                    var masterValue = masterValuesByKey.FirstOrDefault(p => p.Name == value.Name);
                    if (masterValue == null)
                    {
                        await _unitOfWork.Repository<MasterDataValue>().AddAsync(value);
                    }
                    else
                    {
                        masterValue.IsActive = value.IsActive;
                        masterValue.IsDeleted = value.IsDeleted;
                        masterValue.Name = value.Name;
                        _unitOfWork.Repository<MasterDataValue>().Update(masterValue);
                    }
                }
                _unitOfWork.CommitTransaction();
                return true;
            }
        }

        public async Task<List<MasterDataValue>> GetAllMasterValuesAsync()
        {
            var masterValues = await _unitOfWork.Repository<MasterDataValue>().FindAllAsync();
            return masterValues.ToList();
        }
    }
}