// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


namespace Microsoft.IIS.Administration.Core.Http {
    using System.Net;


    public static class ErrorHelper {

        public static dynamic Error(string message) {
            return new {
                title = "Server error",
                detail = message ?? "",
                status = (int)HttpStatusCode.InternalServerError
            };
        }


        public static dynamic ArgumentError(string paramName, string message = null) {

            if (paramName == "model") {
                return new {
                    title = "Invalid JSON request object",
                    status = (int)HttpStatusCode.UnsupportedMediaType
                };
            }

            if (string.IsNullOrEmpty(message)) {
                return new {
                    title = "Invalid parameter",
                    name = paramName,
                    status = (int)HttpStatusCode.BadRequest
                };
            }
            
            return new  {
                title = "Invalid parameter",
                name = paramName,
                detail = message,
                status = (int)HttpStatusCode.BadRequest
            };
        }

        public static dynamic ArgumentOutOfRangeError(string paramName, long min, long max, string message) {
            return new
            {
                title = "Out of range",
                name = paramName,
                min_value = min,
                max_value = max,
                detail = message,
                status = (int)HttpStatusCode.BadRequest
            };
        }

        public static dynamic NotFoundError(string paramName) {
            if (string.IsNullOrEmpty(paramName)) {
                return new {
                    title = "Not found",
                    status = (int)HttpStatusCode.NotFound
                };
            }

            return new {
                title = "Not found",
                name = paramName,
                status = (int)HttpStatusCode.NotFound
            };
        }

        public static dynamic AlreadyExistsError(string paramName) {
            return new {
                title = "Conflict",
                detail = "Already exists",
                name = paramName,
                status = (int)HttpStatusCode.Conflict
            };
        }

        public static dynamic LockedError(string name) {
            return new {
                title = "Object is locked",
                name = name,
                status = (int)HttpStatusCode.Forbidden
            };
        }

        public static dynamic UnauthorizedArgumentError(string paramName) {
            return new {
                title = "Unauthorized",
                name = paramName,
                status = (int)HttpStatusCode.Unauthorized
            };
        }
    }
}
