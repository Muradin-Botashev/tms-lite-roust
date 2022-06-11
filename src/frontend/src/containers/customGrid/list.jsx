import React, { Component } from 'react';
import { connect } from 'react-redux';
import SuperGrid from '../../components/SuperGrid';
import {
    autoUpdateStart,
    autoUpdateStop,
    canCreateByFormSelector,
    listSelector,
    progressSelector,
    totalCountSelector,
} from '../../ducks/gridList';
import { withRouter } from 'react-router-dom';
import Card from './card';
import { Button } from 'semantic-ui-react';
import { withTranslation } from 'react-i18next';
import {
    actionsSelector, allIdsSelector, clearAllIdsRequest,
    getActionsRequest,
    getAllIdsRequest,
    invokeActionRequest,
} from '../../ducks/gridActions';
import {
    editRepresentationRequest,
    getRepresentationsRequest,
    representationFromGridSelector,
    representationNameSelector,
} from '../../ducks/representations';
import {
    DICTIONARY_CARD_LINK,
    DICTIONARY_NEW_LINK,
    GRID_CARD_LINK,
    GRID_NEW_LINK,
} from '../../router/links';
import TableInfo from '../../components/TableInfo';
import CustomCheckbox from "../../components/BaseComponents/CustomCheckbox";
import {startAutoUpdateConfigRequest, stopAutoUpdateConfigRequest} from "../../ducks/profile";

const CreateButton = ({ t, ...res }) => {
    return (
        <Card {...res}>
            <Button icon="add" />
        </Card>
    );
};

class List extends Component {
    constructor(props) {
        super(props);

        this.state = {
            confirmation: { open: false },
        };
    }

    componentWillUnmount() {
        this.props.stopUpdate({isClear: true});
        this.props.stopConfigUpdate();
    }

    mapActions = (item, t, invokeAction, name) => ({
        ...item,
        name: `${t(item.name)}`,
        action: (rows, clearSelectedRows) => {
            this.showConfirmation(
                `${t('Are you sure to complete')} "${t(item.name)}" ${
                    rows.length > 1 ? `${t('for')} ` + rows.length : ''
                }?`,
                () => {
                    this.closeConfirmation();
                    invokeAction({
                        ids: rows,
                        name,
                        actionName: item.name,
                        callbackSuccess: () => {
                            clearSelectedRows();
                        },
                    });
                },
            );
        },
    });

    getGroupActions = () => {
        const { t, actions, invokeAction, match } = this.props;
        const { params = {} } = match;
        const { name = '' } = params;
        const actionsFromGrid = actions.filter(item => item.allowedFromGrid);

        let obj = {
            require: actionsFromGrid
                .filter((item, index) => index < 2)
                .map(item => this.mapActions(item, t, invokeAction, name)),
            other: actionsFromGrid
                .filter((item, index) => index >= 2)
                .map(item => this.mapActions(item, t, invokeAction, name)),
            order: actionsFromGrid
                .filter((item, index) => index >= 3)
                .filter(item => item.group === 'Order')
                .map(item => this.mapActions(item, t, invokeAction, name)),
            shipping: actionsFromGrid
                .filter((item, index) => index >= 3)
                .filter(item => item.group === 'Shipping')
                .map(item => this.mapActions(item, t, invokeAction, name)),
        };

        return obj;
    };

    showConfirmation = (content, onConfirm) => {
        this.setState({ confirmation: { open: true, onConfirm, content } });
    };

    closeConfirmation = () => {
        this.setState({ confirmation: { open: false } });
    };

    modalCard = () => {
        const { stopUpdate, match = {} } = this.props;
        const { params = {} } = match;
        const { name = '' } = params;
        return <Card stopUpdate={stopUpdate} name={name} />;
    };

    render() {
        const {
            columns = [],
            autoUpdate,
            stopUpdate,
            list,
            totalCount,
            progress,
            match = {},
            t,
            isCreateBtn,
            getActions,
            getAllIds,
            editRepresentation,
            representationName,
            getRepresentations,
            startConfigUpdate,
            stopConfigUpdate,
            allIds,
            clearAllIds
        } = this.props;
        const { params = {} } = match;
        const { name = '' } = params;
        const { confirmation } = this.state;

        return (
            <div className="container">
                <SuperGrid
                    key={name}
                    columns={columns}
                    rows={list}
                    name={name}
                    editRepresentation={editRepresentation}
                    representationName={representationName}
                    getRepresentations={getRepresentations}
                    autoUpdateStart={autoUpdate}
                    autoUpdateStop={stopUpdate}
                    totalCount={totalCount}
                    progress={progress}
                    storageSortItem={`${name}Sort`}
                    storageFilterItem={`${name}Filters`}
                    storageRepresentationItems={`${name}Representation`}
                    getActions={getActions}
                    groupActions={this.getGroupActions}
                    getAllIds={getAllIds}
                    modalCard={this.modalCard}
                    isCreateBtn={isCreateBtn}
                    confirmation={confirmation}
                    closeConfirmation={this.closeConfirmation}
                    newLink={isCreateBtn ? GRID_NEW_LINK : null}
                    cardLink={GRID_CARD_LINK}
                    startConfigUpdate={startConfigUpdate}
                    stopConfigUpdate={stopConfigUpdate}
                    allIds={allIds}
                    clearAllIds={clearAllIds}
                />
            </div>
        );
    }
}

const mapDispatchToProps = dispatch => {
    return {
        autoUpdate: params => {
            dispatch(autoUpdateStart(params));
        },
        stopUpdate: params => {
            dispatch(autoUpdateStop(params));
        },
        getActions: params => {
            dispatch(getActionsRequest(params));
        },
        invokeAction: params => {
            dispatch(invokeActionRequest(params));
        },
        getAllIds: params => {
            dispatch(getAllIdsRequest(params));
        },
        editRepresentation: params => {
            dispatch(editRepresentationRequest(params));
        },
        getRepresentations: params => {
            dispatch(getRepresentationsRequest(params));
        },
        startConfigUpdate: () => {
            dispatch(startAutoUpdateConfigRequest())
        },
        stopConfigUpdate: () => {
            dispatch(stopAutoUpdateConfigRequest())
        },
        clearAllIds: params => {
            dispatch(clearAllIdsRequest(params))
        }
    };
};

const mapStateToProps = (state, ownProps) => {
    const { match = {} } = ownProps;
    const { params = {} } = match;
    const { name = '' } = params;

    return {
        columns: representationFromGridSelector(state, name),
        list: listSelector(state),
        totalCount: totalCountSelector(state),
        progress: progressSelector(state),
        isCreateBtn: canCreateByFormSelector(state, name),
        actions: actionsSelector(state),
        representationName: representationNameSelector(state, name),
        allIds: allIdsSelector(state),
    };
};

export default withTranslation()(
    withRouter(
        connect(
            mapStateToProps,
            mapDispatchToProps,
        )(List),
    ),
);
