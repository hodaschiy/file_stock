using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Server.Data;
using Server.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Tests.TestServices
{
    internal class MockCryptService : ICryptService
    {
        public string GetKey(string key) { return "good"; }
        public byte[] EncryptData(byte[] data, string? PublicKey, bool UserKey = false) { return data; }
        public byte[] DecryptData(byte[] data, string? PublicKey, bool UserKey = false) { return data; }
    }

   
}
