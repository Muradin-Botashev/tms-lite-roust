import { createSelector } from 'reselect';
import { postman } from '../utils/postman';
import { all, put, takeEvery } from 'redux-saga/effects';
import { ORDERS_GRID } from '../constants/grids';

const TYPE_API = 'fieldProperties';

//*  TYPES  *//

const GET_FIELDS_SETTINGS_COMPANIES_REQUEST = 'GET_FIELDS_SETTINGS_COMPANIES_REQUEST';
const GET_FIELDS_SETTINGS_COMPANIES_SUCCESS = 'GET_FIELDS_SETTINGS_COMPANIES_SUCCESS';
const GET_FIELDS_SETTINGS_COMPANIES_ERROR = 'GET_FIELDS_SETTINGS_COMPANIES_ERROR';

const GET_FIELDS_SETTINGS_REQUEST = 'GET_FIELDS_SETTINGS_REQUEST';
const GET_FIELDS_SETTINGS_SUCCESS = 'GET_FIELDS_SETTINGS_SUCCESS';
const GET_FIELDS_SETTINGS_ERROR = 'GET_FIELDS_SETTINGS_ERROR';

const EDIT_FIELDS_SETTINGS_REQUEST = 'EDIT_FIELDS_SETTINGS_REQUEST';
const EDIT_FIELDS_SETTINGS_SUCCESS = 'EDIT_FIELDS_SETTINGS_SUCCESS';
const EDIT_FIELDS_SETTINGS_ERROR = 'EDIT_FIELDS_SETTINGS_ERROR';

const TOGGLE_HIDDEN_STATE_REQUEST = 'TOGGLE_HIDDEN_STATE_REQUEST';
const TOGGLE_HIDDEN_STATE_SUCCESS = 'TOGGLE_HIDDEN_STATE_SUCCESS';
const TOGGLE_HIDDEN_STATE_ERROR = 'TOGGLE_HIDDEN_STATE_ERROR';

const CLEAR_FIELDS_SETTINGS = 'CLEAR_FIELDS_SETTINGS';

const DEFAULT_STATE = 'DEFAULT_STATE';

//*  INITIAL STATE  *//

const initial = {
    settings: {},
    progress: false,
    editProgress: false,
};

//*  REDUCER  *//

export default (state = initial, { type, payload = {} }) => {
    switch (type) {
        case GET_FIELDS_SETTINGS_COMPANIES_REQUEST:
            return {
                ...state,
                progress: true,
            };
        case GET_FIELDS_SETTINGS_COMPANIES_SUCCESS:
            return {
                ...state,
                companies: payload,
                progress: false,
            };
        case GET_FIELDS_SETTINGS_COMPANIES_ERROR:
            return {
                ...state,
                companies: {},
                progress: false,
            };
        case GET_FIELDS_SETTINGS_REQUEST:
            return {
                ...state,
                progress: true,
            };
        case GET_FIELDS_SETTINGS_SUCCESS:
            return {
                ...state,
                settings: payload,
                progress: false,
            };
        case EDIT_FIELDS_SETTINGS_REQUEST:
            const { params = {} } = payload;

            return {
                ...state,
                editProgress: {
                    field: params.fieldName,
                    state: params.state,
                },
            };
        case EDIT_FIELDS_SETTINGS_SUCCESS:
        case EDIT_FIELDS_SETTINGS_ERROR:
            return {
                ...state,
                editProgress: false,
            };
        case GET_FIELDS_SETTINGS_ERROR:
            return {
                ...state,
                settings: {},
                progress: false,
            };
        case CLEAR_FIELDS_SETTINGS:
            return {
                ...state,
                ...initial,
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

export const getFieldsSettingCompaniesRequest = payload => {
    return {
        type: GET_FIELDS_SETTINGS_COMPANIES_REQUEST,
        payload,
    };
};

export const getFieldsSettingRequest = payload => {
    return {
        type: GET_FIELDS_SETTINGS_REQUEST,
        payload,
    };
};

export const editFieldsSettingRequest = payload => {
    return {
        type: EDIT_FIELDS_SETTINGS_REQUEST,
        payload,
    };
};

export const clearFieldsSettings = () => {
    return {
        type: CLEAR_FIELDS_SETTINGS,
    };
};

export const toggleHidenStateRequest = payload => {
    return {
        type: TOGGLE_HIDDEN_STATE_REQUEST,
        payload,
    };
};

//*  SELECTORS *//

const stateSelector = state => state.fieldsSetting;

export const fieldsSettingCompaniesSelector = createSelector(stateSelector, state => state.companies);

export const fieldsSettingSelector = createSelector(stateSelector, state => state.settings);

export const progressSelector = createSelector(stateSelector, state => state.progress);

export const editProgressSelector = createSelector(stateSelector, state => state.editProgress);

//*  SAGA  *//
export function* getFieldsSettingCompaniesSaga({ payload }) {
    try {
        const result = yield postman.get(`${TYPE_API}/companies`);
        yield put({
            type: GET_FIELDS_SETTINGS_COMPANIES_SUCCESS,
            payload: result,
        });
    } catch (e) {
        yield put({
            type: GET_FIELDS_SETTINGS_COMPANIES_ERROR,
            payload: e,
        });
    }
}

export function* getFieldsSettingSaga({ payload }) {
    try {
        const baseResult = yield postman.post(`${TYPE_API}/get`, payload);
        const extResult = yield postman.post(`${TYPE_API}/get`, {
            ...payload,
            forEntity: payload.forEntity === ORDERS_GRID ? 'orderItems' : 'routePoints',
        });

        yield put({
            type: GET_FIELDS_SETTINGS_SUCCESS,
            payload: {
                base: baseResult,
                ext: extResult,
            },
        });
    } catch (e) {
        yield put({
            type: GET_FIELDS_SETTINGS_ERROR,
            payload: e,
        });
    }
}

function* editFieldsSettingSaga({ payload = {} }) {
    try {
        const { params, callbackSuccess, isExt } = payload;
        const result = yield postman.post(`/${TYPE_API}/save`, {
            ...params,
            forEntity: isExt
                ? params.forEntity === ORDERS_GRID
                    ? 'orderItems'
                    : 'routePoints'
                : params.forEntity,
        });

        yield put({
            type: EDIT_FIELDS_SETTINGS_SUCCESS,
        });

        callbackSuccess && callbackSuccess();
    } catch (e) {
        yield put({
            type: EDIT_FIELDS_SETTINGS_ERROR,
            payload: e,
        });
    }
}

function* toggleHiddenStateSaga({ payload }) {
    try {
        const { params, callbackSuccess, isExt } = payload;
        const result = yield postman.post(`/${TYPE_API}/toggleHiddenState`, {
            ...params,
            forEntity: isExt
                ? params.forEntity === ORDERS_GRID
                    ? 'orderItems'
                    : 'routePoints'
                : params.forEntity,
        });

        yield put({
            type: TOGGLE_HIDDEN_STATE_SUCCESS,
        });

        callbackSuccess && callbackSuccess();
    } catch (e) {
        yield put({
            type: TOGGLE_HIDDEN_STATE_ERROR,
            payload: e,
        });
    }
}

export function* saga() {
    yield all([
        takeEvery(GET_FIELDS_SETTINGS_COMPANIES_REQUEST, getFieldsSettingCompaniesSaga),
        takeEvery(GET_FIELDS_SETTINGS_REQUEST, getFieldsSettingSaga),
        takeEvery(EDIT_FIELDS_SETTINGS_REQUEST, editFieldsSettingSaga),
        takeEvery(TOGGLE_HIDDEN_STATE_REQUEST, toggleHiddenStateSaga),
    ]);
}
