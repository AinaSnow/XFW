﻿using Sodium;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XIVChatCommon.Message;

namespace XIVChatCommon {
    public static class SecretMessage {
        private const uint MaxMessageLen = 128_000;

        public async static Task<byte[]> ReadSecretMessage(Stream s, byte[] key, CancellationToken token = default) {
            int read = 0;

            byte[] header = new byte[4 + 24];
            while (read < header.Length) {
                read += await s.ReadAsync(header, read, header.Length - read, token);
            }

            uint length = BitConverter.ToUInt32(header, 0);
            byte[] nonce = header.Skip(4).ToArray();

            if (length > MaxMessageLen) {
                throw new ArgumentOutOfRangeException($"Encrypted message specified a size of {length}, which is greater than the limit of {MaxMessageLen}");
            }

            byte[] ciphertext = new byte[length];
            read = 0;
            while (read < ciphertext.Length) {
                read += await s.ReadAsync(ciphertext, read, ciphertext.Length - read, token);
            }

            return SecretBox.Open(ciphertext, nonce, key);
        }

        public async static Task SendSecretMessage(Stream s, byte[] key, byte[] message, CancellationToken token = default) {
            byte[] nonce = SecretBox.GenerateNonce();
            byte[] ciphertext = SecretBox.Create(message, nonce, key);
            byte[] len = BitConverter.GetBytes((uint)ciphertext.Length);

            if (ciphertext.Length > MaxMessageLen) {
                throw new ArgumentOutOfRangeException($"Encrypted message would be {len} bytes long, which is larger than the limit of {MaxMessageLen}");
            }

            await s.WriteAsync(len, 0, len.Length, token);
            await s.WriteAsync(nonce, 0, nonce.Length, token);
            await s.WriteAsync(ciphertext, 0, ciphertext.Length, token);
        }

        public async static Task SendSecretMessage(Stream s, byte[] key, IEncodable message, CancellationToken token = default) {
            await SendSecretMessage(s, key, message.Encode(), token);
        }

        public static int MacSize() => 16;
    }
}
