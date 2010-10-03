/*
 * MindTouch Core - open source enterprise collaborative networking
 * Copyright (c) 2006-2010 MindTouch Inc.
 * www.mindtouch.com  oss@mindtouch.com
 *
 * For community documentation and downloads visit www.opengarden.org;
 * please review the licensing section.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along
 * with this program; if not, write to the Free Software Foundation, Inc.,
 * 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
 * http://www.gnu.org/copyleft/gpl.html
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MindTouch.Deki.Data;
using MindTouch.Deki.Logic;
using MindTouch.Deki.Util;
using MindTouch.Dream;

namespace MindTouch.Deki {
    internal static class DekiExceptionMapper {

        //--- Class Fields ---
        private static readonly Dictionary<Type, MethodInfo> _handlers = new Dictionary<Type, MethodInfo>();

        //--- Class Methods ---

        // TODO: if we need more arguments, need to make the dispatcher smarter to pass in the desired arguments, so that the other handler
        // signatures don't have to change
        public static DreamMessage Map(Exception exception) {
            var exceptionType = exception.GetType();
            MethodInfo handler;
            lock(_handlers) {
                if(!_handlers.TryGetValue(exceptionType, out handler)) {
                    handler = (from method in typeof(DekiExceptionMapper).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                               let parameters = method.GetParameters()
                               where parameters.Length == 1 && parameters[0].ParameterType.IsAssignableFrom(exceptionType)
                               let depth = GetInheritanceChain(parameters[0].ParameterType, 0)
                               orderby depth descending
                               select method).FirstOrDefault();
                    if(handler == null) {
                        return null;
                    }
                    _handlers[exceptionType] = handler;
                }
            }
            return (DreamMessage)handler.Invoke(null, new object[] { exception });
        }

        private static int GetInheritanceChain(Type type, int depth) {
            return type.BaseType == null ? depth : GetInheritanceChain(type.BaseType, ++depth);
        }

        private static DreamMessage Map(ResourceConcurrencyException e) {
            return DreamMessage.Conflict(string.Format(DekiResources.PROPERTY_CONCURRENCY_ERROR, e.ResourceId));
        }

        private static DreamMessage Map(PageConcurrencyException e) {
            return DreamMessage.Conflict(string.Format(DekiResources.PAGE_CONCURRENCY_ERROR, e.PageId));
        }

        private static DreamMessage Map(CommentConcurrencyException e) {
            return DreamMessage.Conflict(string.Format(DekiResources.COMMENT_CONCURRENCY_ERROR, e.PageId));
        }

        private static DreamMessage Map(OldIdNotFoundException e) {
            return DreamMessage.InternalError(string.Format(DekiResources.UNABLE_TO_FIND_OLD_PAGE_FOR_ID, e.OldId, e.TimeStamp));
        }

        private static DreamMessage Map(PageIdNotFoundException e) {
            return DreamMessage.InternalError(string.Format(DekiResources.UNABLE_TO_RETRIEVE_PAGE_FOR_ID, e.PageId));
        }

        private static DreamMessage Map(HomePageNotFoundException e) {
            return DreamMessage.InternalError(DekiResources.UNABLE_TO_FIND_HOME_PAGE);
        }

        private static DreamMessage Map(Exceptions.TalkPageLanguageCannotBeSet e) {
            return DreamMessage.Conflict(DekiResources.LANGUAGE_SET_TALK);
        }

        private static DreamMessage Map(Exceptions.BanEmptyException e) {
            return DreamMessage.BadRequest(DekiResources.BANNING_EMPTY_BAN);
        }

        private static DreamMessage Map(Exceptions.BanNoPermsException e) {
            return DreamMessage.BadRequest(DekiResources.BANNING_NO_PERMS);
        }

        private static DreamMessage Map(Exceptions.InvalidRatingScoreException e) {
            return DreamMessage.BadRequest(DekiResources.RATING_INVALID_SCORE);
        }

        private static DreamMessage Map(Exceptions.ImagePreviewOversizedException e) {
            return DreamMessage.BadRequest(DekiResources.IMAGE_REQUEST_TOO_LARGE);
        }

        private static DreamMessage Map(DekiLicenseException e) {
            return DreamMessage.BadRequest(e.Message);
        }

        private static DreamMessage Map(MindTouchLicenseTransitionException e) {
            return DreamMessage.Forbidden(string.Format(DekiResources.LICENSE_TRANSITION_INVALID, e.CurrentState, e.ProposedState));
        }

        private static DreamMessage Map(MindTouchLicenseInvalidOperationException e) {
            return DreamMessage.Forbidden(string.Format(DekiResources.LICENSE_OPERATION_NOT_ALLOWED, e.Operation));
        }

        private static DreamMessage Map(MindTouchTooManyUsersLicenseException e) {
            return DreamMessage.Forbidden(string.Format(DekiResources.LICENSE_LIMIT_TOO_MANY_USERS, e.CurrentActiveUsers, e.MaxUsers, e.UserDelta));
        }

        private static DreamMessage Map(MindTouchNoNewUserLicenseException e) {
            return DreamMessage.Forbidden(string.Format(DekiResources.LICENSE_NO_NEW_USER_CREATION, e.CurrentLicenseState));
        }

        private static DreamMessage Map(MindTouchLicenseUserCreationException e) {
            return DreamMessage.Forbidden(DekiResources.LICENSE_LIMIT_USER_CREATION);
        }
        private static DreamMessage Map(MindTouchLicenseInstanceException e) {
            return DreamMessage.Forbidden(DekiResources.LICENSE_LIMIT_INSTANCE_CREATION);
        }
    }
}