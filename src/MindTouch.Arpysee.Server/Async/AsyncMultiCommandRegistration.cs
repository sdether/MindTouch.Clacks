﻿/*
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

namespace MindTouch.Arpysee.Server.Async {
    public class AsyncMultiCommandRegistration : AAsyncCommandRegistration<Action<IRequest, Action<IResponse, Action>>> {
        public AsyncMultiCommandRegistration(Action<IRequest, Action<IResponse, Action>> handler, DataExpectation dataExpectation) : base(handler, dataExpectation) {}
        protected override IAsyncCommandHandler BuildHandler(string[] commandArgs, int dataLength, string[] arguments, Action<IRequest, Exception, Action<IResponse>> errorHandler) {
            return new AsyncMultiCommandHandler(commandArgs[0], arguments, dataLength, _handler, errorHandler);
        }
    }
}