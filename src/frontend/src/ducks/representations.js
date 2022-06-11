import { createSelector } from 'reselect';
import { all, put, select, takeEvery } from 'redux-saga/effects';
import { columnsGridSelector } from './gridList';
import { postman } from '../utils/postman';
import { RETURN_COST_TYPE, SOLD_TO_TYPE } from '../constants/columnTypes';

//*  TYPES  *//
const GET_REPRESENTATIONS_REQUEST = 'GET_REPRESENTATIONS_REQUEST';
const GET_REPRESENTATIONS_SUCCESS = 'GET_REPRESENTATIONS_SUCCESS';
const GET_REPRESENTATIONS_ERROR = 'GET_REPRESENTATIONS_ERROR';

const SAVE_REPRESENTATION_REQUEST = 'SAVE_REPRESENTATION_REQUEST';
const SAVE_REPRESENTATION_SUCCESS = 'SAVE_REPRESENTATION_SUCCESS';
const SAVE_REPRESENTATION_ERROR = 'SAVE_REPRESENTATION_ERROR';

const EDIT_REPRESENTATION_REQUEST = 'EDIT_REPRESENTATION_REQUEST';
const EDIT_REPRESENTATION_SUCCESS = 'EDIT_REPRESENTATION_SUCCESS';
const EDIT_REPRESENTATION_ERROR = 'EDIT_REPRESENTATION_ERROR';

const DELETE_REPRESENTATION_REQUEST = 'DELETE_REPRESENTATION_REQUEST';
const DELETE_REPRESENTATION_SUCCESS = 'DELETE_REPRESENTATION_SUCCESS';
const DELETE_REPRESENTATION_ERROR = 'DELETE_REPRESENTATION_REQUEST';

const SET_REPRESENTATION = 'SET_REPRESENTATION';

const REPRESENTATIONS_KEY = 'representations';
const REPRESENTATION_KEY = 'representation';

const CHANGE_IS_OPEN = 'CHANGE_IS_OPEN';

const DEFAULT_STATE = 'DEFAULT_STATE';

//*  INITIAL STATE  *//

const initial = {
    list: {},
    representation: JSON.parse(localStorage.getItem(REPRESENTATION_KEY)) || {},
    open: false,
};

//*  REDUCER  *//

export default (state = initial, { type, payload = {} }) => {
    const { gridName, name, value, oldName } = payload;

    switch (type) {
        case GET_REPRESENTATIONS_SUCCESS:
            return {
                ...state,
                list: payload,
            };
        case SET_REPRESENTATION:
            return {
                ...state,
                representation: {
                    ...state.representation,
                    [gridName]: value,
                },
            };
        case EDIT_REPRESENTATION_SUCCESS:
        case SAVE_REPRESENTATION_SUCCESS:
        case DELETE_REPRESENTATION_SUCCESS:
            return {
                ...state,
                list: payload,
            };
        case CHANGE_IS_OPEN:
            return {
                ...state,
                open: payload,
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

export const getRepresentationsRequest = payload => {
    return {
        type: GET_REPRESENTATIONS_REQUEST,
        payload,
    };
};

export const saveRepresentationRequest = payload => {
    return {
        type: SAVE_REPRESENTATION_REQUEST,
        payload,
    };
};

export const editRepresentationRequest = payload => {
    return {
        type: EDIT_REPRESENTATION_REQUEST,
        payload,
    };
};

export const setRepresentationRequest = payload => {
    return {
        type: SET_REPRESENTATION,
        payload,
    };
};

export const deleteRepresentationRequest = payload => {
    return {
        type: DELETE_REPRESENTATION_REQUEST,
        payload,
    };
};

export const changeIsOpen = payload => {
    return {
        type: CHANGE_IS_OPEN,
        payload,
    };
};

//*  SELECTORS *//

export const stateSelector = state => state.representations;

export const openSelector = createSelector(
    stateSelector,
    state => state.open,
);

export const representationsSelector = createSelector(
    stateSelector,
    state => state.list,
);

export const representationNameSelector = createSelector(
    [stateSelector, (state, name) => name, state => representationsSelector(state)],
    (state, gridName, list) => {
        const name = state.representation[gridName];

        return list[name] ? name : null;
    },
);

export const representationSelector = createSelector(
    [stateSelector, (state, name) => name, (state, name) => columnsGridSelector(state, name)],
    (state, gridName, columnList) => {
        const representationName = state.representation && state.representation[gridName];
        const representation = representationName ? state.list[representationName] : [];
        const actualRepresentation = [];

        representation &&
            representation.forEach(item => {
                const actualItem = columnList.find(column => column.name === item.name);
                if (actualItem) {
                    actualRepresentation.push({
                        ...actualItem,
                        width: item.width,
                        filter: item.filter,
                        sort: item.sort,
                    });
                }
            });
        return actualRepresentation;
    },
);

export const representationFromGridSelector = createSelector(
    [
        stateSelector,
        (state, name) => name,
        (state, name) => columnsGridSelector(state, name),
        (state, name) => representationSelector(state, name),
    ],
    (state, gridName, list, representation) => {
        if (representation && representation.length) {
            return representation;
        }
        return list.filter(item => item.isDefault);
    },
);

export const columnsTypesConfigSelector = createSelector(
    columnsGridSelector,
    columns => {
        let config = {};

        columns.forEach(column => {
            config = {
                ...config,
                [column.name]: {
                    type: column.name === 'soldTo' ? SOLD_TO_TYPE : column.type,
                    source: column.source,
                },
            };
        });

        return {
            ...config,
            orderCosts: {
                type: RETURN_COST_TYPE,
            },
        };
    },
);

//*  SAGA  *//

function* getRepresentationsSaga({ payload }) {
    try {
        const { key, callBackFunc } = payload;
        const result = yield postman.get(`/userSettings/${key}`);

        yield put({
            type: GET_REPRESENTATIONS_SUCCESS,
            payload: result.value ? JSON.parse(result.value) : {},
        });
        const columns = yield select(state => representationFromGridSelector(state, key));

        callBackFunc && callBackFunc(columns);
    } catch (e) {
        yield put({
            type: GET_REPRESENTATIONS_ERROR,
            payload: e,
        });
    }
}

function* saveRepresentationSaga({ payload }) {
    try {
        const { callbackSuccess, key, name, value } = payload;
        const list = yield select(representationsSelector);

        const params = {
            ...list,
            [name]: value.map(item => ({
                ...item,
            })),
        };

        const result = yield postman.post(`/userSettings/${key}`, {
            value: JSON.stringify(params),
        });

        yield put({
            type: SAVE_REPRESENTATION_SUCCESS,
            payload: params,
        });

        callbackSuccess && callbackSuccess();
    } catch (e) {
        yield put({
            type: SAVE_REPRESENTATION_ERROR,
            payload: e,
        });
    }
}

function* editRepresentationSaga({ payload }) {
    try {
        const { callbackSuccess, key, name, value, oldName } = payload;
        const list = yield select(representationsSelector);

        let params = {};

        Object.keys(list).forEach(keyList => {
            if (keyList !== oldName) {
                params = {
                    ...params,
                    [keyList]: list[keyList],
                };
            }
        });

        params = {
            ...params,
            [name]: value,
        };

        const result = yield postman.post(`/userSettings/${key}`, {
            value: JSON.stringify(params),
        });

        yield put({
            type: EDIT_REPRESENTATION_SUCCESS,
            payload: params,
        });

        callbackSuccess && callbackSuccess();
    } catch (e) {
        yield put({
            type: EDIT_REPRESENTATION_ERROR,
            payload: e,
        });
    }
}

function* deleteRepresentationSaga({ payload }) {
    try {
        const { callbackSuccess, key, name } = payload;
        const list = yield select(representationsSelector);

        let params = {};

        Object.keys(list).forEach(keyList => {
            if (keyList !== name) {
                params = {
                    ...params,
                    [keyList]: list[keyList],
                };
            }
        });

        const result = yield postman.post(`/userSettings/${key}`, {
            value: JSON.stringify(params),
        });

        yield put({
            type: DELETE_REPRESENTATION_SUCCESS,
            payload: params,
        });

        callbackSuccess && callbackSuccess();
    } catch (e) {
        yield put({
            type: DELETE_REPRESENTATION_ERROR,
            payload: e,
        });
    }
}

function* setRepresentationSaga({ payload }) {
    try {
        yield put(
            getRepresentationsRequest({
                key: payload.gridName,
            }),
        );
        const { callbackSuccess } = payload;
        const state = yield select(state => state.representations.representation);
        localStorage.setItem(REPRESENTATION_KEY, JSON.stringify(state));

        setTimeout(() => {
            callbackSuccess && callbackSuccess();
        }, 500);
    } catch (e) {
        console.log('___error', e);
    }
}

export function* saga() {
    yield all([
        takeEvery(SAVE_REPRESENTATION_REQUEST, saveRepresentationSaga),
        takeEvery(EDIT_REPRESENTATION_REQUEST, editRepresentationSaga),
        takeEvery(DELETE_REPRESENTATION_REQUEST, deleteRepresentationSaga),
        takeEvery(SET_REPRESENTATION, setRepresentationSaga),
        takeEvery(GET_REPRESENTATIONS_REQUEST, getRepresentationsSaga),
    ]);
}
