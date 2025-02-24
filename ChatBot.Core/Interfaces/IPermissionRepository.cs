﻿using ChatBot.Core.Models;
using System.Collections.Generic;

namespace ChatBot.Core.Interfaces
{
    public interface IPermissionRepository
    {
        IEnumerable<Permission> Get(bool trackChanges);
        Permission GetById(int id, bool trackChanges);
        Permission GetByName(string name, bool trackChanges);
        void CreatePermission(Permission permission);
        void DeletePermission(Permission permission);
        void UpdatePermission(Permission permission);
    }
}
