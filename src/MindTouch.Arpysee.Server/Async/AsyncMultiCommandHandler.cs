/*
 * MindTouch.Arpysee
 * 
 * Copyright (C) 2011 Arne F. Claassen
 * geekblog [at] claassen [dot] net
 * http://github.com/sdether/MindTouch.Arpysee
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;

namespace MindTouch.Arpysee.Server.Async {
    public class AsyncMultiCommandHandler : IAsyncCommandHandler {

        private readonly string _command;
        private readonly string[] _arguments;
        private readonly Action<IRequest, Action<IResponse, Action>> _handler;
        private readonly Action<IRequest, Exception, Action<IResponse>> _errorHandler;
        private readonly int _dataLength;
        private readonly bool _disconnect;
        private int _received;
        private List<byte[]> _dataChunks;

        public AsyncMultiCommandHandler(string command, string[] arguments, int dataLength, Action<IRequest, Action<IResponse, Action>> handler, Action<IRequest, Exception, Action<IResponse>> errorHandler) {
            _command = command;
            _arguments = arguments;
            _dataLength = dataLength;
            _handler = handler;
            _errorHandler = errorHandler;
        }

        public void Dispose() { }

        public bool ExpectsData { get { return _dataLength > 0; } }
        public bool DisconnectOnCompletion { get { return _disconnect; } }
        public int OutstandingBytes { get { return _dataLength - _received; } }
        public string Command { get { return _command; } }

        public void AcceptData(byte[] chunk) {
            if(_dataChunks == null) {
                _dataChunks = new List<byte[]>();
            }
            _dataChunks.Add(chunk);
            _received += chunk.Length;
            if(_received > _dataLength) {
                throw new DataExpectationException(true);
            }
        }

        public void GetResponse(Action<IResponse, Action> responseCallback) {
            if(_received < _dataLength) {
                throw new DataExpectationException(false);
            }
            var request = new Request(_command, _arguments, _dataLength, _dataChunks);

            // Note: the recursive action assumes that responseCallback will call the recursiveAction callback from a new
            // thread each time (i.e. post async IO), otherwise this can turn into a stack overflow.
            Action recursiveAction = null;
            recursiveAction = () => {
                try {
                    _handler(request, (response, callback) => {
                        if(callback == null) {
                            responseCallback(response, null);
                        } else {
                            responseCallback(response, recursiveAction);
                        }

                    });
                } catch(Exception e) {
                    _errorHandler(request, e, response => responseCallback(response, null));
                }
            };
            recursiveAction();
        }
    }
}