import { createSelector } from 'reselect';
import { postman } from '../utils/postman';
import { all, put, takeEvery } from 'redux-saga/effects';
import { toast } from 'react-toastify';

//*  TYPES  *//

const CHECK_FOR_EDITING_REQUEST = 'CHECK_FOR_EDITING_REQUEST';
const CHECK_FOR_EDITING_SUCCESS = 'CHECK_FOR_EDITING_SUCCESS';
const CHECK_FOR_EDITING_ERROR = 'CHECK_FOR_EDITING_ERROR';

const DEFAULT_STATE = 'DEFAULT_STATE';

//*  INITIAL STATE  *//

const initial = {
    checkProgress: false,
    editModal: false,
};

//*  REDUCER  *//

export default (state = initial, { type, payload = {} }) => {
    switch (type) {
        case CHECK_FOR_EDITING_REQUEST:
            return {
                ...state,
                checkProgress: {
                    rowId: payload.rowId,
                    fieldName: payload.fieldName,
                },
                editModal: false,
            };
        case CHECK_FOR_EDITING_SUCCESS:
            return {
                ...state,
                checkProgress: false,
                editModal: { ...payload },
            };
        case CHECK_FOR_EDITING_ERROR:
            return {
                ...state,
                checkProgress: false,
                editModal: false,
            };
        case DEFAULT_STATE:
            return {
                ...initial
            };
        default:
            return state;
    }
};

//*  ACTION CREATORS  *//

export const checkForEditingRequest = payload => {
    return {
        type: CHECK_FOR_EDITING_REQUEST,
        payload,
    };
};

//*  SELECTORS *//

const stateSelector = state => state.gridColumnEdit;
export const checkProgressSelector = createSelector(stateSelector, state => state.checkProgress);
export const editModalSelector = createSelector(stateSelector, state => state.editModal);

//*  SAGA  *//

function* checkForEditingSaga({ payload }) {
    try {
        const { forEntity, fieldName, state, callbackSuccess, t, callbackFunc } = payload;
        const result = yield postman.post('/fieldProperties/getField', {
            forEntity,
            fieldName,
            state,
        });

        if (result.accessType === 'edit') {
            yield put({
                type: CHECK_FOR_EDITING_SUCCESS,
                payload,
            });

            callbackSuccess && callbackSuccess();
        } else {
            toast.error(t(`check_for_editing_failed_${forEntity}`, { fieldName: t(fieldName) }));
            yield put({
                type: CHECK_FOR_EDITING_ERROR,
                payload,
            });
        }
        callbackFunc && callbackFunc();
    } catch (e) {
        yield put({
            type: CHECK_FOR_EDITING_ERROR,
            payload: e,
        });
    }
}

export function* saga() {
    yield all([takeEvery(CHECK_FOR_EDITING_REQUEST, checkForEditingSaga)]);
}
