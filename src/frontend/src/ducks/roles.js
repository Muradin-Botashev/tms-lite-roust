import { all, put, takeEvery } from 'redux-saga/effects';
import { postman } from '../utils/postman';
import { createSelector } from 'reselect';
import roles from '../mocks/roles';
import { toast } from 'react-toastify';
import { errorMapping } from '../utils/errorMapping';

const TYPE_API = 'roles';

//*  TYPES  *//

const GET_ROLES_LIST_REQUEST = 'GET_ROLES_LIST_REQUEST';
const GET_ROLES_LIST_SUCCESS = 'GET_ROLES_LIST_SUCCESS';
const GET_ROLES_LIST_ERROR = 'GET_ROLES_LIST_ERROR';

const GET_ROLE_CARD_REQUEST = 'GET_ROLE_CARD_REQUEST';
const GET_ROLE_CARD_SUCCESS = 'GET_ROLE_CARD_SUCCESS';
const GET_ROLE_CARD_ERROR = 'GET_ROLE_CARD_ERROR';

const CREATE_ROLE_REQUEST = 'CREATE_ROLE_REQUEST';
const CREATE_ROLE_SUCCESS = 'CREATE_ROLE_SUCCESS';
const CREATE_ROLE_ERROR = 'CREATE_ROLE_ERROR';

const TOGGLE_ROLE_ACTIVE_REQUEST = 'TOGGLE_ROLE_ACTIVE_REQUEST';
const TOGGLE_ROLE_ACTIVE_SUCCESS = 'TOGGLE_ROLE_ACTIVE_SUCCESS';
const TOGGLE_ROLE_ACTIVE_ERROR = 'TOGGLE_ROLE_ACTIVE_ERROR';

const GET_ALL_PERMISSIONS_REQUEST = 'GET_ALL_PERMISSIONS_REQUEST';
const GET_ALL_PERMISSIONS_SUCCESS = 'GET_ALL_PERMISSIONS_SUCCESS';
const GET_ALL_PERMISSIONS_ERROR = 'GET_ALL_PERMISSIONS_ERROR';

const GET_ALL_ACTIONS_REQUEST = 'GET_ALL_ACTIONS_REQUEST';
const GET_ALL_ACTIONS_SUCCESS = 'GET_ALL_ACTIONS_SUCCESS';
const GET_ALL_ACTIONS_ERROR = 'GET_ALL_ACTIONS_ERROR';

const GET_ALL_BACKLIGHTS_REQUEST = 'GET_ALL_BACKLIGHTS_REQUEST';
const GET_ALL_BACKLIGHTS_SUCCESS = 'GET_ALL_BACKLIGHTS_SUCCESS';
const GET_ALL_BACKLIGHTS_ERROR = 'GET_ALL_BACKLIGHTS_ERROR';

const CLEAR_ROLE_CARD = 'CLEAR_ROLE_CARD';

const CLEAR_ERROR = 'CLEAR_ERROR';

const DEFAULT_STATE = 'DEFAULT_STATE';

//*  INITIAL STATE  *//

const initial = {
    list: [],
    card: {},
    error: [],
    permissions: [],
    actions: [],
    backlights: [],
    totalCount: 0,
    progress: false,
};

//*  REDUCER  *//

export default (state = initial, { type, payload }) => {
    switch (type) {
        case GET_ROLES_LIST_REQUEST:
        case GET_ROLE_CARD_REQUEST:
        case CREATE_ROLE_REQUEST:
            return {
                ...state,
                progress: true,
                error: [],
            };
        case GET_ROLES_LIST_SUCCESS:
            return {
                ...state,
                progress: false,
                list: payload.isConcat ? [...state.list, ...payload.items] : payload.items,
                totalCount: payload.totalCount,
            };
        case GET_ROLE_CARD_SUCCESS:
            return {
                ...state,
                progress: false,
                card: payload,
            };
        case GET_ROLES_LIST_ERROR:
        case GET_ROLE_CARD_ERROR:
            return {
                ...state,
                progress: false,
            };
        case CREATE_ROLE_SUCCESS:
            return {
                ...state,
                progress: false,
                error: [],
            };
        case CREATE_ROLE_ERROR:
            return {
                ...state,
                progress: false,
                error: payload,
            };
        case CLEAR_ROLE_CARD:
            return {
                ...state,
                card: {},
                error: [],
            };
        case CLEAR_ERROR:
            return {
                ...state,
                error: state.error && state.error.filter(item => item.name !== payload),
            };
        case GET_ALL_PERMISSIONS_SUCCESS:
            return {
                ...state,
                permissions: payload,
            };
        case GET_ALL_ACTIONS_SUCCESS:
            return {
                ...state,
                actions: payload,
            };
        case GET_ALL_BACKLIGHTS_SUCCESS:
            return {
                ...state,
                backlights: payload,
            };
        case GET_ALL_ACTIONS_ERROR:
            return {
                ...state,
                actions: {},
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

export const getRolesRequest = payload => {
    return {
        type: GET_ROLES_LIST_REQUEST,
        payload,
    };
};

export const getRoleCardRequest = payload => {
    return {
        type: GET_ROLE_CARD_REQUEST,
        payload,
    };
};

export const createRoleRequest = payload => {
    return {
        type: CREATE_ROLE_REQUEST,
        payload,
    };
};

export const clearRolesInfo = () => {
    return {
        type: DEFAULT_STATE,
    };
};

export const clearRolesCard = () => {
    return {
        type: CLEAR_ROLE_CARD,
    };
};

export const toggleRoleActiveRequest = payload => {
    return {
        type: TOGGLE_ROLE_ACTIVE_REQUEST,
        payload,
    };
};

export const getAllPermissionsRequest = payload => {
    return {
        type: GET_ALL_PERMISSIONS_REQUEST,
        payload,
    };
};

export const getAllActionsRequest = payload => {
    return {
        type: GET_ALL_ACTIONS_REQUEST,
        payload,
    };
};

export const clearError = payload => {
    return {
        type: CLEAR_ERROR,
        payload,
    };
};

export const getAllBacklightsRequest = payload => {
    return {
        type: GET_ALL_BACKLIGHTS_REQUEST,
        payload,
    };
};

//*  SELECTORS *//

const stateSelector = state => state.roles;

export const rolesListSelector = createSelector(stateSelector, state => state.list);

export const rolesFromUserSelector = createSelector(stateSelector, state => {
    return (
        state.list &&
        state.list.map(item => ({
            name: item.name,
            value: item.id,
            isActive: true,
        }))
    );
});

export const progressSelector = createSelector(stateSelector, state => state.progress);
export const totalCountSelector = createSelector(stateSelector, state => state.totalCount);
export const roleCardSelector = createSelector(stateSelector, state => state.card);
export const errorSelector = createSelector(stateSelector, state => errorMapping(state.error));

export const allPermissionsSelector = createSelector(stateSelector, state => state.permissions);
export const allActionsSelector = createSelector(stateSelector, state => state.actions);
export const allBacklightsSelector = createSelector(stateSelector, state => state.backlights);

//*  SAGA  *//

function* getRolesListSaga({ payload }) {
    try {
        const { filter = {}, isConcat } = payload;
        const result = yield postman.post(`/${TYPE_API}/search`, filter);

        yield put({
            type: GET_ROLES_LIST_SUCCESS,
            payload: {
                ...result,
                isConcat,
            },
        });
    } catch (error) {
        yield put({
            type: GET_ROLES_LIST_ERROR,
            payload: error,
        });
    }
}

function* getRoleCardSaga({ payload }) {
    try {
        const result = yield postman.get(`/${TYPE_API}/getById/${payload}`);

        yield put({
            type: GET_ROLE_CARD_SUCCESS,
            payload: result,
        });
    } catch (error) {
        yield put({
            type: GET_ROLE_CARD_ERROR,
            payload: error,
        });
    }
}

function* createRoleSaga({ payload }) {
    try {
        const { params, callbackFunc } = payload;

        const result = yield postman.post(`/${TYPE_API}/saveOrCreate`, params);

        if (result.isError) {
            toast.error(result.message);
            yield put({
                type: CREATE_ROLE_ERROR,
                payload: result.errors,
            });
        } else {
            yield put({
                type: CREATE_ROLE_SUCCESS,
            });
            callbackFunc && callbackFunc();
        }
    } catch (error) {
        yield put({
            type: CREATE_ROLE_ERROR,
        });
    }
}

function* toggleRoleActiveSaga({ payload }) {
    try {
        const { id, active, callbackSuccess } = payload;
        const result = yield postman.post(`/${TYPE_API}/setActive/${id}/${active}`);

        yield put({
            type: TOGGLE_ROLE_ACTIVE_SUCCESS,
        });
        callbackSuccess && callbackSuccess();
    } catch (e) {
        yield put({
            type: TOGGLE_ROLE_ACTIVE_ERROR,
            payload: e,
        });
    }
}

function* getAllPermissionsSaga({ payload }) {
    try {
        const result = yield postman.get(`/${TYPE_API}/allPermissions`);

        yield put({
            type: GET_ALL_PERMISSIONS_SUCCESS,
            payload: result,
        });
    } catch (e) {
        yield put({
            type: GET_ALL_PERMISSIONS_ERROR,
            payload: e,
        });
    }
}

function* getAllActionsSaga({ payload }) {
    try {
        const result = yield postman.get(`/${TYPE_API}/allActions`);

        yield put({
            type: GET_ALL_ACTIONS_SUCCESS,
            payload: result,
        });
    } catch (e) {
        yield put({
            type: GET_ALL_ACTIONS_ERROR,
            payload: e,
        });
    }
}

function* getAllBacklightsSaga() {
    try {
        const result = yield postman.get(`${TYPE_API}/allBacklights`);

        yield put({
            type: GET_ALL_BACKLIGHTS_SUCCESS,
            payload: result,
        });
    } catch (e) {
        yield put({
            type: GET_ALL_BACKLIGHTS_ERROR,
        });
    }
}

export function* saga() {
    yield all([
        takeEvery(GET_ROLES_LIST_REQUEST, getRolesListSaga),
        takeEvery(GET_ROLE_CARD_REQUEST, getRoleCardSaga),
        takeEvery(CREATE_ROLE_REQUEST, createRoleSaga),
        takeEvery(TOGGLE_ROLE_ACTIVE_REQUEST, toggleRoleActiveSaga),
        takeEvery(GET_ALL_PERMISSIONS_REQUEST, getAllPermissionsSaga),
        takeEvery(GET_ALL_ACTIONS_REQUEST, getAllActionsSaga),
        takeEvery(GET_ALL_BACKLIGHTS_REQUEST, getAllBacklightsSaga),
    ]);
}
