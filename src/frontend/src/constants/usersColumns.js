import {ACTIVE_TYPE, ENUM_TYPE, SELECT_TYPE, TEXT_TYPE} from './columnTypes';

export const usersColumns = [
    {
        name: 'login',
        displayNameKey: 'login',
    },
    {
        name: 'email',
        displayNameKey: 'email',
        type: TEXT_TYPE
    },
    {
        name: 'userName',
        displayNameKey: 'userName',
        type: TEXT_TYPE
    },
    {
        name: 'roleId',
        displayNameKey: 'role',
        type: SELECT_TYPE,
        source: 'roles',
    },
    {
        name: 'carrierId',
        displayNameKey: 'carrierId',
        type: SELECT_TYPE,
        source: 'transportCompanies',
    },
    {
        name: 'companyId',
        displayNameKey: 'companyId',
        type: SELECT_TYPE,
        source: 'companies',
    },
    {
        name: 'isActive',
        displayNameKey: 'isActive',
        type: ACTIVE_TYPE,
    },
];
