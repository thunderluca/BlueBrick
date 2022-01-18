// BlueBrick, a LEGO(c) layout editor.
// Copyright (C) 2008 Alban NANTY
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// see http://www.fsf.org/licensing/licenses/gpl.html
// and http://www.gnu.org/licenses/
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

namespace BlueBrick
{
    public partial class MainForm
    {
        // a mapping key table to store the shortcut for each action
        enum ShortcutableAction
        {
            ADD_PART = 0,
            DELETE_PART,
            ROTATE_LEFT,
            ROTATE_RIGHT,
            MOVE_LEFT,
            MOVE_RIGHT,
            MOVE_UP,
            MOVE_DOWN,
            CHANGE_CURRENT_CONNEXION,
            SEND_TO_BACK,
            BRING_TO_FRONT,
            NB_ACTIONS
        };
    }
}