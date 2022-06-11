import { all, put, takeEvery } from 'redux-saga/effects';
import { createSelector } from 'reselect';
import { postman } from '../utils/postman';

//*  TYPES  *//
const GET_HISTORY_REQUEST = 'GET_HISTORY_REQUEST';
const GET_HISTORY_SUCCESS = 'GET_HISTORY_SUCCESS';
const GET_HISTORY_ERROR = 'GET_HISTORY_ERROR';

const CLEAR_HISTORY = 'CLEAR_HISTORY';
const DEFAULT_STATE = 'DEFAULT_STATE';

//*  INITIAL STATE  *//

const initial = {
    data: [],
    progress: false,
};

//*  REDUCER  *//

export default (state = initial, { type, payload }) => {
    switch (type) {
        case GET_HISTORY_REQUEST:
            return {
                ...state,
                progress: true,
            };
        case GET_HISTORY_SUCCESS:
            return {
                ...state,
                progress: false,
                data: payload,
            };
        case GET_HISTORY_ERROR:
            return {
                ...state,
                progress: false,
                data: [],
            };
        case CLEAR_HISTORY:
            return {
                ...state,
                data: [],
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

export const getHistoryRequest = payload => {
    return {
        type: GET_HISTORY_REQUEST,
        payload,
    };
};

export const clearHistory = () => {
    return {
        type: CLEAR_HISTORY,
    };
};

//*  SELECTORS *//

const stateSelector = state => state.historyList;

export const historySelector = createSelector(stateSelector, state => state.data);

export const progressSelector = createSelector(stateSelector, state => state.progress);

//*  SAGA  *//

function* getHistorySaga({ payload }) {
    try {
        const result = yield postman.get(`/history/${payload}`);
        yield put({
            type: GET_HISTORY_SUCCESS,
            payload: result.entries,
        });
    } catch (e) {
        yield put({
            type: GET_HISTORY_ERROR,
            payload: e,
        });
    }
}

export function* saga() {
    yield all([takeEvery(GET_HISTORY_REQUEST, getHistorySaga)]);
}
