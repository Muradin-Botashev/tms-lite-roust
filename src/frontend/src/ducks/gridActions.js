import { createSelector } from 'reselect';
import { postman } from '../utils/postman';
import { all, put, select, takeEvery } from 'redux-saga/effects';
import { toast } from 'react-toastify';
import { representationFromGridSelector } from './representations';
import {showModal} from "./modal";

//*  TYPES  *//

const GET_IDS_REQUEST = 'GET_IDS_REQUEST';
const GET_IDS_SUCCESS = 'GET_IDS_SUCCESS';
const GET_IDS_ERROR = 'GET_IDS_ERROR';

const GET_ACTIONS_REQUEST = 'GET_ACTIONS_REQUEST';
const GET_ACTIONS_SUCCESS = 'GET_ACTIONS_SUCCESS';
const GET_ACTIONS_ERROR = 'GET_ACTIONS_ERROR';

const INVOKE_ACTION_REQUEST = 'INVOKE_ACTION_REQUEST';
const INVOKE_ACTION_SUCCESS = 'INVOKE_ACTION_SUCCESS';
const INVOKE_ACTION_ERROR = 'INVOKE_ACTION_ERROR';

const INVOKE_MASS_UPDATE_REQUEST = 'INVOKE_MASS_UPDATE_REQUEST';
const INVOKE_MASS_UPDATE_SUCCESS = 'INVOKE_MASS_UPDATE_SUCCESS';
const INVOKE_MASS_UPDATE_ERROR = 'INVOKE_MASS_UPDATE_ERROR';

const CLEAR_ACTIONS = 'CLEAR_ACTIONS';

const DEFAULT_STATE = 'DEFAULT_STATE';

//*  INITIAL STATE  *//

const initial = {
    actions: [],
    actionsCard: [],
    info: {},
    updates: [],
    ids: [],
    progress: false,
    progressActionName: null,
    progressMassUpdate: false,
};

//*  REDUCER  *//

export default (state = initial, { type, payload }) => {
    switch (type) {
        case GET_IDS_REQUEST:
            return {
                ...state,
                actions: [],
                info: [],
                updates: [],
            };
        case GET_IDS_SUCCESS:
            return {
                ...state,
                ids: payload
            };
        case GET_ACTIONS_REQUEST:
            return {
                ...state,
                actions: [],
                progress: true
            };
        case GET_ACTIONS_SUCCESS:
            let stateNew = {
                ...state,
                info: payload.info,
                updates: payload.updates,
                progress: false,
            };

            if (payload.isCard) {
                stateNew = {
                    ...stateNew,
                    actionsCard: payload.actions,
                };
            } else {
                stateNew = {
                    ...stateNew,
                    actions: payload.actions,
                };
            }

            return stateNew;
        case GET_ACTIONS_ERROR:
            return {
                ...state,
                actions: [],
                progress: false
            };
        case INVOKE_ACTION_REQUEST:
            return {
                ...state,
                progressActionName: payload.actionName,
            };
        case INVOKE_ACTION_SUCCESS:
            return {
                ...state,
                progressActionName: null,
            };
        case INVOKE_ACTION_ERROR:
            return {
                ...state,
                progressActionName: null,
            };
        case INVOKE_MASS_UPDATE_REQUEST:
            return {
                ...state,
                progressMassUpdate: true,
            };
        case INVOKE_MASS_UPDATE_SUCCESS:
        case INVOKE_MASS_UPDATE_ERROR:
            return {
                ...state,
                progressMassUpdate: false,
            };
        case CLEAR_ACTIONS:
            return {
                ...state,
                actions: [],
                info: {},
                updates: [],
                actionsCard: [],
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

export const getActionsRequest = payload => {
    return {
        type: GET_ACTIONS_REQUEST,
        payload,
    };
};

export const invokeActionRequest = payload => {
    return {
        type: INVOKE_ACTION_REQUEST,
        payload,
    };
};

export const clearActions = () => {
    return {
        type: CLEAR_ACTIONS,
    };
};

export const getAllIdsRequest = payload => {
    return {
        type: GET_IDS_REQUEST,
        payload,
    };
};

export const invokeMassUpdateRequest = payload => {
    return {
        type: INVOKE_MASS_UPDATE_REQUEST,
        payload,
    };
};

export const clearAllIdsRequest = payload => {
    return {
        type: GET_IDS_SUCCESS,
        payload
    }
};

//*  SELECTORS *//

const stateSelector = state => state.gridActions;

export const actionsSelector = createSelector(stateSelector, state =>
    state.actions.map(item => ({
        ...item,
        ids: item.ids || [],
    })),
);

export const actionsCardSelector = createSelector(stateSelector, state =>
    (state.actionsCard || []).map(item => ({
        ...item,
        ids: item.ids || [],
    })),
);

export const progressSelector = createSelector(stateSelector, state => state.progress);

export const progressActionNameSelector = createSelector(
    stateSelector,
    state => state.progressActionName,
);

export const infoSelector = createSelector(stateSelector, state => state.info);
export const updatesSelector = createSelector(stateSelector, state => state.updates);
export const progressMassUpdateSelector = createSelector(
    stateSelector,
    state => state.progressMassUpdate,
);

export const allIdsSelector = createSelector(stateSelector, state => state.ids);

//*  SAGA  *//

function* getActionsSaga({ payload }) {
    try {
        const { name, ids = [], isCard } = payload;
        if (ids.length) {
            const actions = yield postman.post(`/${name}/getActions`, ids);
            let info = {};
            let updates = [];

            if (!isCard) {
                info = yield postman.post(`/${name}/getSummary`, ids);
                updates = yield postman.post(`/${name}/getBulkUpdates`, ids);
            }
            yield put({
                type: GET_ACTIONS_SUCCESS,
                payload: { actions, info, updates, isCard },
            });
        } else {
            yield put({
                type: GET_ACTIONS_SUCCESS,
                payload: { actions: [], info: {}, update: [], actionsCard: [] },
            });
        }
    } catch (e) {
        yield put({ type: GET_ACTIONS_ERROR });
    }
}

function* invokeActionSaga({ payload }) {
    try {
        const { ids, callbackSuccess, name, actionName } = payload;
        const result = yield postman.post(`/${name}/invokeAction/${actionName}`, ids);
        yield put({
            type: INVOKE_ACTION_SUCCESS,
        });

        if (result.messageType === 0) {
            toast.info(result.message, {
                autoClose: !result.manuallyClosableMessage,
            });
        } else {
            yield put(showModal(result));
        }

        callbackSuccess && callbackSuccess();
    } catch (e) {
        yield put({
            type: INVOKE_ACTION_ERROR,
            payload: e,
        });
    }
}

function* getAllIdsSaga({ payload }) {
    try {
        const { filter, name, callbackSuccess } = payload;
        const representation = yield select(state => representationFromGridSelector(state, name));
        const columns = representation ? representation.map(item => item.name) : [];
        const result = yield postman.post(`/${name}/ids`, {
            ...filter,
            filter: {
                ...filter.filter,
                columns,
            },
        });

        yield put({
            type: GET_IDS_SUCCESS,
            payload: result
        });

        callbackSuccess && callbackSuccess(result);
    } catch (e) {
        yield put({
            type: GET_IDS_ERROR,
            payload: e,
        });
    }
}

function* invokeMassUpdateSaga({ payload }) {
    try {
        const { ids, callbackSuccess, name, field, value } = payload;
        const result = yield postman.post(`/${name}/invokeBulkUpdate/${field}`, {
            ids,
            value: value && typeof value === 'object' ? value.value : value,
        });
        if (result.isError) {
            toast.error(result.message);
            yield put({
                type: INVOKE_MASS_UPDATE_ERROR,
                payload: result,
            });
        } else {
            yield put({
                type: INVOKE_MASS_UPDATE_SUCCESS,
            });
            result.message && toast.info(result.message);
        }
        callbackSuccess && callbackSuccess();
    } catch (e) {
        yield put({
            type: INVOKE_MASS_UPDATE_ERROR,
            payload: e,
        });
    }
}

export function* saga() {
    yield all([
        takeEvery(GET_ACTIONS_REQUEST, getActionsSaga),
        takeEvery(INVOKE_ACTION_REQUEST, invokeActionSaga),
        takeEvery(GET_IDS_REQUEST, getAllIdsSaga),
        takeEvery(INVOKE_MASS_UPDATE_REQUEST, invokeMassUpdateSaga),
    ]);
}
