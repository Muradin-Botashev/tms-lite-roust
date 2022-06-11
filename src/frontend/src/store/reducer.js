import { combineReducers } from 'redux';

import { connectRouter } from 'connected-react-router';
import { default as profile } from '../ducks/profile';
import { default as login } from '../ducks/login';
import { default as roles } from '../ducks/roles';
import { default as gridList } from '../ducks/gridList';
import { default as dictionaryView } from '../ducks/dictionaryView';
import { default as gridActions } from '../ducks/gridActions';
import { default as gridCard } from '../ducks/gridCard';
import { default as lookup } from '../ducks/lookup';
import { default as documents } from '../ducks/documents';
import { default as representations } from '../ducks/representations';
import { default as historyList } from '../ducks/history';
import { default as fieldsSetting } from '../ducks/fieldsSetting';
import { default as gridColumnEdit } from '../ducks/gridColumnEdit';
import { default as loadingData } from '../ducks/loadingData';
import { default as modal } from '../ducks/modal';
import { default as autogrouping } from '../ducks/autogrouping';
import { default as reports } from '../ducks/reports';
import { default as users } from '../ducks/users';

export default history =>
    combineReducers({
        login,
        gridList,
        profile,
        roles,
        users,
        dictionaryView,
        gridActions,
        gridCard,
        lookup,
        documents,
        representations,
        historyList,
        fieldsSetting,
        gridColumnEdit,
        loadingData,
        modal,
        autogrouping,
        reports,
        router: connectRouter(history),
    });
