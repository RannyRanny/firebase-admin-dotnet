// Copyright 2018, Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FirebaseAdmin.Auth;
using FirebaseAdmin.Tests;
using Xunit;

namespace FirebaseAdmin.Auth.Tests
{
    public class HttpPublicKeySourceTest
    {
        [Fact]
        public async Task GetPublicKeysWithoutCaching()
        {
            var clock = new MockClock();
            var handler = new MockMessageHandler()
            {
                Response = File.ReadAllBytes("./resources/public_keys.json"),
            };
            var clientFactory = new MockHttpClientFactory(handler);
            var keyManager = new HttpPublicKeySource(
                "https://example.com/certs", clock, clientFactory);
            var keys = await keyManager.GetPublicKeysAsync();
            Assert.Equal(2, keys.Count);
            Assert.Equal(1, handler.Calls);

            var keys2 = await keyManager.GetPublicKeysAsync();
            Assert.Equal(2, keys.Count);
            Assert.Equal(2, handler.Calls);
            Assert.NotSame(keys, keys2);
        }

        [Fact]
        public async Task GetPublicKeysWithCaching()
        {
            var clock = new MockClock();
            var cacheControl = new CacheControlHeaderValue()
            {
                MaxAge = new TimeSpan(hours: 1, minutes: 0, seconds: 0),
            };
            var handler = new MockMessageHandler()
            {
                Response = File.ReadAllBytes("./resources/public_keys.json"),
                ApplyHeaders = (header, _) => header.CacheControl = cacheControl,
            };
            var clientFactory = new MockHttpClientFactory(handler);
            var keyManager = new HttpPublicKeySource(
                "https://example.com/certs", clock, clientFactory);
            var keys = await keyManager.GetPublicKeysAsync();
            Assert.Equal(2, keys.Count);
            Assert.Equal(1, handler.Calls);

            clock.UtcNow = clock.UtcNow.AddMinutes(50);
            var keys2 = await keyManager.GetPublicKeysAsync();
            Assert.Equal(2, keys.Count);
            Assert.Equal(1, handler.Calls);
            Assert.Same(keys, keys2);

            clock.UtcNow = clock.UtcNow.AddMinutes(10);
            var keys3 = await keyManager.GetPublicKeysAsync();
            Assert.Equal(2, keys.Count);
            Assert.Equal(2, handler.Calls);
            Assert.NotSame(keys, keys3);
        }

        [Fact]
        public void InvalidArguments()
        {
            var clock = new MockClock();
            var handler = new MockMessageHandler()
            {
                Response = File.ReadAllBytes("./resources/public_keys.json"),
            };
            var clientFactory = new MockHttpClientFactory(handler);
            Assert.Throws<ArgumentException>(
                () => new HttpPublicKeySource(null, clock, clientFactory));
            Assert.Throws<ArgumentException>(
                () => new HttpPublicKeySource(string.Empty, clock, clientFactory));
            Assert.Throws<ArgumentNullException>(
                () => new HttpPublicKeySource("https://example.com/certs", null, clientFactory));
            Assert.Throws<ArgumentNullException>(
                () => new HttpPublicKeySource("https://example.com/certs", clock, null));
        }

        [Fact]
        public async Task HttpError()
        {
            var clock = new MockClock();
            var handler = new MockMessageHandler()
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Response = "test error",
            };
            var clientFactory = new MockHttpClientFactory(handler);
            var keyManager = new HttpPublicKeySource(
                "https://example.com/certs", clock, clientFactory);
            await Assert.ThrowsAsync<FirebaseException>(
                async () => await keyManager.GetPublicKeysAsync());
            Assert.Equal(1, handler.Calls);
        }
    }
}
