import React, { useEffect } from 'react';
import {
    Button,
    Checkbox,
    Dimmer,
    Form,
    Icon,
    Input,
    Loader,
    Popup,
    Visibility,
} from 'semantic-ui-react';

import { connect } from 'react-redux';
import { withTranslation } from 'react-i18next';
import { clearLookup, getLookupRequest, listSelector, progressSelector } from '../../ducks/lookup';

class Facet extends React.Component {
    state = {};

    setFilter = (e, { value }) => {
        this.setState({ filter: value.toLowerCase() });
    };

    clearFilter = () => {
        this.setState({ filter: '' });
        this.props.clearLookup();
    };

    handleOpen = () => {
        const { source, getList } = this.props;

        getList({
            name: source,
            params: {},
        });
    };

    componentDidMount() {
        this.handleOpen();
    }

    componentWillUnmount() {
        this.clearFilter();
    }

    handleRestClick = () => {
        const { name, onChange } = this.props;

        if (onChange !== undefined) onChange(null, { name: name, value: null });
    };

    toggle = (e, { value }) => {
        const { name, onChange } = this.props;
        let values = this.props.value ? this.props.value.split('|') : [];

        if (values.some(x => x === value)) {
            values.splice(values.indexOf(value), 1);
        } else {
            values.push(value);
        }
        if (onChange !== undefined) onChange(e, { name: name, value: values.join('|') });
    };

    render() {
        const { value, valuesList = [], loading, t } = this.props;

        let values = value ? value.split('|') : [];

        let items = valuesList.filter(item => item.name) || [];

        items = items.map(x => {
            return {
                value: x.value,
                name: t(x.name),
                isActive: x.isActive,
            };
        });

        if (this.state.filter)
            items = items.filter(
                x => values.includes(x.value) || x.name.toLowerCase().includes(this.state.filter),
            );

        return (
            <div className="facet-input">
                <Form>
                    {/*<label className="label-in-popup">{t(name)}</label>*/}
                    <div>
                        <Input
                            fluid
                            size="mini"
                            icon="search"
                            value={this.state.filter}
                            onChange={this.setFilter}
                        />
                    </div>
                    <div className="reset-selected">
                        <span onClick={this.handleRestClick}>{t('reset_selected')}</span>
                    </div>
                    <div className="select-facet-values">
                        <Dimmer active={loading} inverted>
                            <Loader size="small">Loading</Loader>
                        </Dimmer>
                        <div>
                            {items &&
                                items.map(x => {
                                    let label = <label>{x.name}</label>;
                                    return (
                                        <Form.Field
                                            key={x.value}
                                            className={!x.isActive ? 'colorGrey' : ''}
                                        >
                                            <Checkbox
                                                value={x.value}
                                                checked={values.includes(x.value)}
                                                onChange={this.toggle}
                                                label={label}
                                            />
                                        </Form.Field>
                                    );
                                })}
                            <Visibility
                                continuous={false}
                                once={false}
                                context={context || undefined}
                                onTopVisible={onBottomVisible}
                                style={{
                                    position: 'absolute',
                                    bottom: 0,
                                    left: 0,
                                    right: 0,
                                    zIndex: -1,
                                }}
                            />
                        </div>
                    </div>
                </Form>
            </div>
        );
    }
}

const mapStateToProps = state => {
    return {
        valuesList: listSelector(state),
        loading: progressSelector(state),
    };
};

const mapDispatchToProps = dispatch => {
    return {
        getList: params => {
            dispatch(getLookupRequest(params));
        },
        clearLookup: () => {
            dispatch(clearLookup());
        },
    };
};

export default withTranslation()(
    connect(
        mapStateToProps,
        mapDispatchToProps,
    )(Facet),
);
