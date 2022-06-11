import React from 'react';
import { useSelector } from 'react-redux';
import Information from './orderTabs/information';
import Position from './orderTabs/position';
import Documents from './shared/documents';
import History from './shared/history';

import CardLayout from '../../../components/CardLayout';
import { userPermissionsSelector } from '../../../ducks/profile';

const OrderCard = ({
    form,
    onChangeForm,
    name,
    id,
    load,
    isNotUniqueNumber,
    uniquenessNumberCheck,
    settings,
    error,
    title,
    onClose,
    actionsFooter,
    actionsHeader,
    loading,
    goToSoldTo,
}) => {
    const userPermissions = useSelector(state => userPermissionsSelector(state));

    const getPanes = () => {
        let obj = [
            {
                menuItem: 'information',
                render: () => (
                    <Information
                        form={form}
                        settings={settings}
                        error={error}
                        load={load}
                        isNotUniqueNumber={isNotUniqueNumber}
                        uniquenessNumberCheck={uniquenessNumberCheck}
                        onChange={onChangeForm}
                        goToSoldTo={goToSoldTo}
                    />
                ),
            },
            {
                menuItem: 'position',
                render: () => (
                    <Position
                        form={form}
                        onChange={onChangeForm}
                        gridName={name}
                        load={load}
                        error={error}
                        settings={settings}
                    />
                ),
            },
        ];
        /*{
                menuItem: 'returns',
                render: () => (
                    <Returns
                        form={form}
                        settings={settings}
                        error={error}
                        onChange={onChangeForm}
                    />
                ),
            },*/

        if ((userPermissions.includes(4) || userPermissions.includes(5)) && form.id) {
            obj.push({
                menuItem: 'documents',
                render: () => (
                    <Documents
                        gridName={name}
                        cardId={id}
                        load={load}
                        isEditPermissions={userPermissions.includes(5)}
                    />
                ),
            });
        }

        if (userPermissions.includes(6) && form.id) {
            obj.push({
                menuItem: 'history',
                render: () => <History cardId={id} status={form.status} />,
            });
        }

        return obj;
    };

    return (
        <CardLayout
            title={title}
            actionsFooter={actionsFooter}
            actionsHeader={actionsHeader}
            content={getPanes}
            onClose={onClose}
            loading={loading}
        />
    );
};

export default OrderCard;
