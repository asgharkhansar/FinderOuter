﻿// The FinderOuter
// Copyright (c) 2020 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Encoders;

namespace FinderOuter.Services
{
    public class AddressService
    {
        private readonly Address addrMan = new Address();

        /// <summary>
        /// Checks the given address and returns its decoded hash.
        /// Works only for P2PKH and P2WPKH addresses
        /// </summary>
        public bool CheckAndGetHash(string address, out byte[] hash)
        {
            hash = null;
            if (string.IsNullOrWhiteSpace(address))
            {
                return false;
            }

            if (address[0] == '1')
            {
                return addrMan.VerifyType(address, PubkeyScriptType.P2PKH, out hash);
            }
            else if (address[0] == 'b')
            {
                return addrMan.VerifyType(address, PubkeyScriptType.P2WPKH, out hash);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks the given address and returns its decoded hash.
        /// Works only for P2SH addresses
        /// </summary>
        public bool CheckAndGetHash_P2sh(string address, out byte[] hash)
        {
            if (string.IsNullOrWhiteSpace(address) || address[0] != '3')
            {
                hash = null;
                return false;
            }
            else
            {
                return addrMan.VerifyType(address, PubkeyScriptType.P2SH, out hash);
            }
        }
    }
}
