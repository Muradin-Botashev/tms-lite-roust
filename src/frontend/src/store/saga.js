import { all } from 'redux-saga/effects';
import { saga as profileSaga } from '../ducks/profile';
import { saga as loginSaga } from '../ducks/login';
import { saga as rolesSaga } from '../ducks/roles';
import { saga as gridListSaga } from '../ducks/gridList';
import { saga as dictionaryViewSaga } from '../ducks/dictionaryView';
import { saga as gridActionsSaga } from '../ducks/gridActions';
import { saga as gridCardSaga } from '../ducks/gridCard';
import { saga as lookupSaga } from '../ducks/lookup';
import { saga as documentsSaga } from '../ducks/documents';
import { saga as representationsSaga } from '../ducks/representations';
import { saga as historySaga } from '../ducks/history';
import { saga as fieldsSettingSaga } from '../ducks/fieldsSetting';
import { saga as gridColumnEditSaga } from '../ducks/gridColumnEdit';
import { saga as usersSaga } from '../ducks/users';
import { saga as loadingDataSaga } from '../ducks/loadingData';
import { saga as autogroupingSaga } from '../ducks/autogrouping';
import { saga as reportsSaga } from '../ducks/reports';
import { saga as generalSaga } from '../ducks/general';

export default function* rootSaga() {
    yield all([
        reportsSaga(),
        autogroupingSaga(),
        loadingDataSaga(),
        gridColumnEditSaga(),
        fieldsSettingSaga(),
        historySaga(),
        representationsSaga(),
        documentsSaga(),
        lookupSaga(),
        gridCardSaga(),
        gridActionsSaga(),
        dictionaryViewSaga(),
        gridListSaga(),
        profileSaga(),
        loginSaga(),
        rolesSaga(),
        usersSaga(),
        generalSaga(),
    ]);
}
